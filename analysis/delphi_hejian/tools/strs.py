import pefile,sys,re
f,out=sys.argv[1],sys.argv[2]
pe=pefile.PE(f); ib=pe.OPTIONAL_HEADER.ImageBase
res=[]
for s in pe.sections:
    nm=s.Name.rstrip(b'\0').decode()
    if nm in ('.rsrc','.reloc','.idata','.edata'): continue
    d=s.get_data(); va=ib+s.VirtualAddress
    # ascii / gbk strings null-terminated
    for m in re.finditer(rb'(?:[\x20-\x7e\t\r\n]|[\x81-\xfe][\x40-\xfe]){4,}\x00',d):
        b=m.group()[:-1]
        try: t=b.decode('gbk')
        except: t=b.decode('latin1')
        res.append((va+m.start(),nm,'A',t))
    for m in re.finditer(rb'(?:[\x20-\x7e]\x00){4,}\x00\x00',d):
        if m.start()%2: continue
        t=m.group()[:-2].decode('utf-16le')
        res.append((va+m.start(),nm,'W',t))
res.sort()
with open(out,'w',encoding='utf-8') as w:
    for a,nm,k,t in res: w.write('%08X %s %s %s\n'%(a,nm,k,t.replace('\r','\r').replace('\n','\n')))
