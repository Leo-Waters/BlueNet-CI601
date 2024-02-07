import random
raw=""
comp=""

minutes=10

direction=True

sent=0
recived=0
for x in range(60*minutes):
    
    if(direction):
        sent-=random.randint(1, 400)
        recived+=random.randint(1, 500)
        direction=False
    else:
        sent+=random.randint(1, 400)
        recived-=random.randint(1, 500)
        direction=True

    if(sent<0):
        sent=0
    if(recived<0):
        recived=0

    
    if(raw==""):
        raw+=str(sent)+" "+str(recived)+" 10 "+str(random.randint(58,60))
    else:
        raw+="\n"+str(sent)+" "+str(recived)+" 10 "+str(random.randint(58,60))

    sent=round(sent/2)
    recived=round(recived/2)
    if(comp==""):
        comp+=str(sent)+" "+str(recived)+" 10 "+str(random.randint(58,60))
    else:
        comp+="\n"+str(sent)+" "+str(recived)+" 10 "+str(random.randint(58,60))


f = open("Comp_Data.txt", "w")
f.write(comp)
f.close()

f = open("Full_Size_Data.txt", "w")
f.write(raw)
f.close()
