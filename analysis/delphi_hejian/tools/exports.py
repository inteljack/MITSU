import pefile,sys
f,out=sys.argv[1],sys.argv[2]
pe=pefile.PE(f)
with open(out,'w',encoding='utf-8') as w:
  for x in sorted(pe.DIRECTORY_ENTRY_EXPORT.symbols,key=lambda x:x.address):
    w.write('%08X %4d %s\n'%(x.address+pe.OPTIONAL_HEADER.ImageBase,x.ordinal,(x.name or b'').decode('latin1')))
  if hasattr(pe,'DIRECTORY_ENTRY_IMPORT'):
    for e in pe.DIRECTORY_ENTRY_IMPORT:
      for i in e.imports:
        w.write('IMP %08X %s!%s\n'%(i.address,e.dll.decode(),(i.name or b'#%d'%i.ordinal).decode()))
