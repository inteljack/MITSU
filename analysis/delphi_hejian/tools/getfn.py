import sys,re
p,name=sys.argv[1],sys.argv[2]
on=False
for l in open(p,encoding='utf-8'):
    if l.startswith('=========='):
        on = name in l
        if on: print(l.rstrip())
        continue
    if on: print(l.rstrip())
