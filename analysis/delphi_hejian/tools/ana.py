import pefile,sys,re,struct,bisect,json
from capstone import *
f,out=sys.argv[1],sys.argv[2]
pe=pefile.PE(f); ib=pe.OPTIONAL_HEADER.ImageBase
img=pe.get_memory_mapped_image()
def rd(va,n): return img[va-ib:va-ib+n]
def d32(va): return struct.unpack('<I',rd(va,4))[0]
text=[s for s in pe.sections if s.Name.startswith(b'.text')][0]
tva=ib+text.VirtualAddress; tsz=text.Misc_VirtualSize
code=rd(tva,tsz)
# strings
strs={}
for s in pe.sections:
    if s.Name.rstrip(b'\0') not in (b'.data',): continue
    d=rd(ib+s.VirtualAddress,s.Misc_VirtualSize)
    for m in re.finditer(rb'(?:[\x20-\x7e\t\r\n]|[\x81-\xfe][\x40-\xfe]){2,}\x00',d):
        b=m.group()[:-1]
        try:t=b.decode('gbk')
        except:t=b.decode('latin1')
        strs[ib+s.VirtualAddress+m.start()]=t
# imports
imp={}
for e in pe.DIRECTORY_ENTRY_IMPORT:
    for i in e.imports: imp[i.address]=e.dll.decode().split('.')[0]+'!'+(i.name.decode() if i.name else '#%d'%i.ordinal)
thunk={}
for m in re.finditer(rb'\xff\x25(....)',code):
    a=struct.unpack('<I',m.group(1))[0]
    if a in imp: thunk[tva+m.start()]=imp[a]
# names: exports
names={}
for x in pe.DIRECTORY_ENTRY_EXPORT.symbols:
    if x.name: names[ib+x.address]=x.name.decode('latin1')
# VMT method tables: find class name shortstrings 'Tfrm...'/'TSendThread'
handlers={}
fields={}
for m in re.finditer(rb'[\x05-\x20](T(?:frm|Send|Login|Password)[A-Za-z]+)',img):
    nameva=ib+m.start(); cname=m.group(1).decode()
    if img[m.start()]!=len(cname): continue
    for mm in re.finditer(re.escape(struct.pack('<I',nameva)),img):
        vmt=ib+mm.start()+44
        if d32(vmt-76)!=vmt: continue
        mt=d32(vmt-52)
        if not mt: continue
        cnt=struct.unpack('<H',rd(mt,2))[0]; p=mt+2
        for i in range(cnt):
            sz=struct.unpack('<H',rd(p,2))[0]; addr=d32(p+2); n=rd(p+6,1)[0]; nm=rd(p+7,n).decode()
            handlers[addr]=cname+'::'+nm; p+=sz
        names.setdefault(vmt,'VMT '+cname)
        ft=d32(vmt-56)
        if ft:
            cnt=struct.unpack('<H',rd(ft,2))[0]; p=ft+6
            for i in range(cnt):
                off=d32(p); n=rd(p+6,1)[0]; nm=rd(p+7,n).decode(); fields.setdefault(cname,{})[off]=nm; p+=7+n
for a,n in handlers.items(): names[a]=n
# function starts: push ebp; mov ebp,esp
starts=sorted(set([tva+m.start() for m in re.finditer(rb'\x55\x8b\xec',code)]+[a for a in names if tva<=a<tva+tsz]))
md=Cs(CS_ARCH_X86,CS_MODE_32); md.detail=False
funcs={}
for i,s in enumerate(starts):
    e=starts[i+1] if i+1<len(starts) else tva+tsz
    if e-s>0x20000: e=s+0x20000
    ins=list(md.disasm(rd(s,e-s),s))
    funcs[s]=ins
# label functions by log string 'xxx---Name:'
def strref(op):
    out=[]
    for h in re.findall(r'0x([0-9a-f]{6,8})',op):
        v=int(h,16)
        if v in strs: out.append(strs[v])
    return out
fname={}
for s,ins in funcs.items():
    for i in ins:
        for t in strref(i.op_str):
            if '---' in t and t.endswith(':') and s not in fname: fname[s]=t[:-1]
for s in funcs:
    if s in names: fname[s]=names[s]+((' ['+fname[s]+']') if s in fname else '')
# calls graph
def tgt(i):
    if i.mnemonic in ('call','jmp'):
        m=re.match(r'0x([0-9a-f]+)$',i.op_str)
        if m: return int(m.group(1),16)
        m=re.match(r'dword ptr \[0x([0-9a-f]+)\]$',i.op_str)
        if m and int(m.group(1),16) in imp: return ('I',imp[int(m.group(1),16)])
    return None
units=sorted([(a,n.split('@')[2]) for a,n in names.items() if n.startswith('@@U') and n.endswith('Initialize')])
cmap={'Ufrmmain':'TfrmMain','Ufrmctrl':'TfrmCtrl','Ufrmprintman':'TfrmPrintMan','Ufrmdataselect':'TfrmDataSelect','Ufrmcarton':'TfrmCarton','Ufrmsystem':'TfrmSystem','Ufrmlogin':'TfrmLogin','Ufrmsuperset':'TfrmSuperset','Ufrmproduct':'TfrmProduct','Ufrmpreview':'TfrmPreview','Ufrmstatus':'TfrmStatus','Usendthread':'TSendThread','Ufrmabout':'TfrmAbout'}
def unitcls(a):
    for ua,un in units:
        if a<ua: return cmap.get(un)
    return None
with open(out+'.fields.txt','w',encoding='utf-8') as w:
    for c,fs in fields.items():
        w.write('== '+c+chr(10))
        for o in sorted(fs): w.write('  +%03X %s'%(o,fs[o])+chr(10))
with open(out+'.summary.txt','w',encoding='utf-8') as w, open(out+'.disasm.txt','w',encoding='utf-8') as wd:
    for s in sorted(funcs):
        ins=funcs[s]
        ss=[];calls=[]
        for i in ins:
            ss+=strref(i.op_str)
            t=tgt(i)
            if isinstance(t,tuple): calls.append(t[1])
            elif t in thunk: calls.append(thunk[t])
            elif t in fname and i.mnemonic=='call': calls.append(fname[t])
        if not (s in fname or ss or any('AIMDTL' in c for c in calls)): continue
        w.write('\n== %08X %s (len %x)\n'%(s,fname.get(s,'?'),(ins[-1].address-s) if ins else 0))
        if calls: w.write('  calls: '+', '.join(dict.fromkeys(calls))+'\n')
        if ss: w.write('  strs: '+' | '.join(x.replace('\n','\n') for x in ss)+'\n')
        if s>=0x401000 and s<0x45e000 or s in fname:
            wd.write('\n========== %08X %s\n'%(s,fname.get(s,'?')))
            for i in ins:
                c=''
                st=strref(i.op_str)
                cl=unitcls(s)
                if cl in fields:
                    for h in re.findall(r'\+ 0x([0-9a-f]{3})\]',i.op_str):
                        if int(h,16) in fields[cl]: c+=' ; .'+fields[cl][int(h,16)]
                if st: c+=' ; "'+'" "'.join(st)+'"'
                t=tgt(i)
                if isinstance(t,tuple): c+=' ; '+t[1]
                elif t in thunk: c+=' ; '+thunk[t]
                elif t in fname: c+=' ; '+fname[t]
                wd.write('%08X  %-6s %s%s\n'%(i.address,i.mnemonic,i.op_str,c))
json.dump({'handlers':{('%08X'%k):v for k,v in handlers.items()},'fname':{('%08X'%k):v for k,v in fname.items()}},open(out+'.names.json','w',encoding='utf-8'),ensure_ascii=False,indent=0)
print(len(funcs),len(handlers),len(fname))
