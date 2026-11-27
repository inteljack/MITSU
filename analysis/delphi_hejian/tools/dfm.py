import pefile,sys,struct,os
def dec(b):
    for enc in ('gbk','big5','cp1252'):
        try: return b.decode(enc)
        except: pass
    return b.decode('latin1')
class R:
    def __init__(s,d): s.d=d; s.p=0
    def u8(s): v=s.d[s.p]; s.p+=1; return v
    def ss(s): n=s.u8(); v=s.d[s.p:s.p+n]; s.p+=n; return v.decode('latin1')
    def take(s,n): v=s.d[s.p:s.p+n]; s.p+=n; return v
def q(st): 
    return "'"+st.replace("'","''")+"'"
def value(r,ind):
    t=r.u8()
    if t==0: return 'null'
    if t==1:
        items=[]
        while r.d[r.p]!=0: items.append(value(r,ind+2))
        r.p+=1; return '('+' '.join(items)+')'
    if t==2: return str(struct.unpack('b',r.take(1))[0])
    if t==3: return str(struct.unpack('<h',r.take(2))[0])
    if t==4: return str(struct.unpack('<i',r.take(4))[0])
    if t==5:
        b=r.take(10); m=int.from_bytes(b[:8],'little'); e=int.from_bytes(b[8:],'little'); sg=-1 if e&0x8000 else 1; e&=0x7fff
        return repr(0.0 if m==0 else sg*m*2.0**(e-16383-63))
    if t==6: n=r.u8(); return q(dec(r.take(n)))
    if t==7: return r.ss()
    if t==8: return 'False'
    if t==9: return 'True'
    if t==10: n=struct.unpack('<i',r.take(4))[0]; b=r.take(n); return '{binary %d bytes: %s...}'%(n,b[:24].hex())
    if t==11:
        items=[]
        while True:
            s=r.ss()
            if not s: break
            items.append(s)
        return '['+', '.join(items)+']'
    if t==12: n=struct.unpack('<i',r.take(4))[0]; return q(dec(r.take(n)))
    if t==13: return 'nil'
    if t==14:
        out=['<']
        while r.d[r.p]!=0:
            if r.d[r.p] in (2,3,4): out.append(' '*(ind+2)+'[%s]'%value(r,ind))
            out.append(' '*(ind+2)+'item')
            if r.d[r.p]==1: r.p+=1
            while r.d[r.p]!=0:
                nm=r.ss(); out.append(' '*(ind+4)+nm+' = '+value(r,ind+4))
            r.p+=1; out.append(' '*(ind+2)+'end')
        r.p+=1; return '\n'.join(out)+'>'
    if t==15: return repr(struct.unpack('<f',r.take(4))[0])
    if t==16: return str(struct.unpack('<q',r.take(8))[0]/10000)
    if t==17: return 'date:'+r.take(8).hex()
    if t==18: n=struct.unpack('<i',r.take(4))[0]; return q(r.take(2*n).decode('utf-16le'))
    if t==19: return str(struct.unpack('<q',r.take(8))[0])
    if t==20: n=struct.unpack('<i',r.take(4))[0]; return q(r.take(n).decode('utf8'))
    raise Exception('vt %d at %x'%(t,r.p))
def comp(r,ind,out):
    b=r.d[r.p]; kw='object'
    if b&0xF0==0xF0:
        fl=r.u8()&0x0f
        if fl&2:
            value(r,ind)
        if fl&1: kw='inherited'
        if fl&4: kw='inline'
    cls=r.ss(); nm=r.ss()
    out.append(' '*ind+'%s %s: %s'%(kw,nm,cls))
    while r.d[r.p]!=0:
        p=r.ss(); out.append(' '*(ind+2)+p+' = '+value(r,ind+2))
    r.p+=1
    while r.d[r.p]!=0: comp(r,ind+2,out)
    r.p+=1
    out.append(' '*ind+'end')
f,od=sys.argv[1],sys.argv[2]; os.makedirs(od,exist_ok=True)
pe=pefile.PE(f)
for t in pe.DIRECTORY_ENTRY_RESOURCE.entries:
    if t.id!=10: continue
    for n in t.directory.entries:
        for l in n.directory.entries:
            de=l.data.struct; data=pe.get_data(de.OffsetToData,de.Size)
            name=str(n.name)
            if data[:4]!=b'TPF0':
                open(os.path.join(od,name+'.bin'),'wb').write(data); continue
            r=R(data); r.p=4; out=[]
            try: comp(r,0,out)
            except Exception as e: out.append('!!PARSE ERROR %s'%e)
            open(os.path.join(od,name+'.dfm.txt'),'w',encoding='utf-8').write('\n'.join(out)+'\n')
            print(name,len(data),'rva=%x'%de.OffsetToData,len(out))
