from pathlib import Path
import matplotlib.pyplot as plt
import sys
import os
class DataSet:
  def __init__(self,name,seconds,read, sent,objUpdates,fps):
    self.name=name
    self.seconds = seconds
    self.read = read
    self.sent = sent
    self.objUpdates = objUpdates
    self.fps = fps

def LoadDataFromFile(path):
    read=[]
    sent=[]
    objUpdates=[]
    fps=[]
    seconds=[]

    second=0
    
    f = open(path,'r') 
    for row in f:
        second+=1
        seconds.append(second)
        row = row.split(' ') 
        read.append(int(row[0])) 
        sent.append(int(row[1]))
        objUpdates.append(int(row[2])) 
        fps.append(int(row[3]))


    
        
    data=DataSet(os.path.splitext(os.path.basename(path))[0],seconds,read,sent,objUpdates,fps);
    return data


print("\n\nCreating graph for: "+os.getcwd()+"/"+sys.argv[1]+"\n\n")

workingDir=os.getcwd()+"/"+sys.argv[1];


TestDataSets=[]

for root, dirs, files in os.walk(workingDir):
    for f in files:
        if os.path.splitext(f)[1] == '.txt':
            fullpath = os.path.join(root, f)
            print("Loading :"+f)
            TestDataSets.append(LoadDataFromFile(fullpath))
fig = plt.figure()

BytesSentGraph = fig.add_subplot(411,label = "Bytes Sent",ylabel='Bytes')
BytesReadGraph = fig.add_subplot(412,sharex = BytesSentGraph, sharey = BytesSentGraph,label = "Bytes Read",ylabel='Bytes')
UpdatesGraph = fig.add_subplot(413,sharex = BytesSentGraph,label = "Object Updates Recived",ylabel='updates')
FPSGraph = fig.add_subplot(414,sharex = BytesSentGraph,label = "FPS",ylabel='Frames')


BytesSentGraph.set_title('Bytes Sent')

for data in TestDataSets:
  BytesSentGraph.plot(data.seconds, data.sent, label = data.name)


BytesSentGraph.get_xaxis().set_visible(False)
BytesSentGraph.legend(loc='upper center', bbox_to_anchor=(0.2, 1.35),fancybox=True, shadow=True, ncol=5)


BytesReadGraph.set_title('Bytes Recived')
for data in TestDataSets:
  BytesReadGraph.plot(data.seconds, data.read, label = data.name)

BytesReadGraph.get_xaxis().set_visible(False)

UpdatesGraph.set_title('Object Updates recived')
for data in TestDataSets:
  UpdatesGraph.plot(data.seconds, data.objUpdates, label = data.name)
  
UpdatesGraph.get_xaxis().set_visible(False)

FPSGraph.set_title('Frames per second')
for data in TestDataSets:
  FPSGraph.plot(data.seconds, data.fps, label = data.name)


FPSGraph.set_xlabel('Seconds')

plt.show()
