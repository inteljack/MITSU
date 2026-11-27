import pefile,re,os,sys,hashlib,datetime
sys.path.insert(0,os.path.dirname(__file__))
B=r'D:/dev_program/MITSU/鶴見'
vers=['2.1.0.7','2.1.0.8','2.1.0.9','2.1.1.0','2.1.1.1','2.1.1.3','2.1.1.4','2.1.1.5','2.1.1.6','2.1.1.7','2.1.1.8','2.1.1.9','2.1.1.10','2.1.1.11','2.1.1.12','2.1.1.14','2.1.1.15','2.1.1.16','2.1.1.17','2.1.1.19','2.1.2.1','2.1.2.2','2.1.2.3','2.1.2.4','2.1.2.5','2.1.2.6']
paths=[(v,os.path.join(B,'hejian_'+v,'hejian.exe')) for v in vers]+[('20160530',r'D:/dev_program/MITSU/鶴見-出差紀錄/鶴見/改流水號判斷/hejian_20160530-2.exe')]
def info(p):
    pe=pefile.PE(p)
    ts=datetime.datetime.utcfromtimestamp(pe.FILE_HEADER.TimeDateStamp)
    d=b''.join(s.get_data() for s in pe.sections if s.Name.startswith(b'.data'))
    strs=set()
    for m in re.finditer(rb'(?:[\x20-\x7e]|[\x81-\xfe][\x40-\xfe]){4,}\x00',d):
        b=m.group()[:-1]
        try:t=b.decode('gbk')
        except:continue
        strs.add(t)
    ver=None
    try:
        for fi in pe.FileInfo[0]:
            if hasattr(fi,'StringTable'):
                for st in fi.StringTable: ver=st.entries.get(b'FileVersion')
    except Exception: pass
    forms={}
    for t in pe.DIRECTORY_ENTRY_RESOURCE.entries:
        if t.id==10:
            for n in t.directory.entries:
                de=n.directory.entries[0].data.struct
                forms[str(n.name)]=hashlib.md5(pe.get_data(de.OffsetToData,de.Size)).hexdigest()[:8]
    imps=sorted(i.name.decode() for e in pe.DIRECTORY_ENTRY_IMPORT if e.dll.upper()==b'AIMDTL.DLL' for i in e.imports if i.name)
    return ts,ver,strs,forms,imps,os.path.getsize(p)
prev=None
for v,p in paths:
    if not os.path.exists(p): print(v,'MISSING'); continue
    ts,ver,strs,forms,imps,sz=info(p)
    print('=== %s size=%d linkts=%s FileVersion=%s'%(v,sz,ts,ver))
    if prev:
        ps,pf,pi=prev
        app=lambda s:{x for x in s if ('---' in x or re.match(r'^[A-Za-z]{3,40}$',x) or any(k in x for k in ('select','insert','update','delete','create','where','[','Provider','msg','.ms','.xls'))) and len(x)<200}
        a=app(strs)-app(ps); r=app(ps)-app(strs)
        if a: print('  + '+' | '.join(sorted(a))[:3000])
        if r: print('  - '+' | '.join(sorted(r))[:3000])
        fc=[k for k in forms if pf.get(k)!=forms[k]]
        print('  forms changed:',fc, 'added:',set(forms)-set(pf),'removed:',set(pf)-set(forms))
        if imps!=pi: print('  aimdtl imports:',set(imps)^set(pi))
    else:
        print('  aimdtl imports:',imps); print('  forms',sorted(forms))
    prev=(strs,forms,imps)
