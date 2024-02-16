from pathlib import Path
import matplotlib.pyplot as plt
import numpy as np
import sys
import os

#Data Set object representing the data of a compression test
class DataSet:
  def __init__(self,name,seconds,read, sent,objUpdates,fps,latency,averageSent,averageRecived,averageLatency,averageFps):
    self.name=name
    self.seconds = seconds
    self.read = read
    self.sent = sent
    self.objUpdates = objUpdates
    self.fps = fps
    self.latency=latency
    self.averageRecived=averageRecived
    self.averageSent=averageSent
    self.averageLatency=averageLatency
    self.averageFps=averageFps

#load a data set from a file path
def LoadDataFromFile(path):
    read=[]
    sent=[]
    objUpdates=[]
    fps=[]
    seconds=[]
    latency=[]
    second=0

    totalSent=0
    totalRecived=0
    totalFps=0
    totalLatency=0
    
    f = open(path,'r') 
    for row in f:
        second+=1
        seconds.append(second)
        row = row.split(' ')
        
        readI=int(row[0])
        sentI=int(row[1])
        totalSent=totalSent+sentI
        totalRecived=totalRecived+readI
        read.append(readI) 
        sent.append(sentI)
        
        objUpdates.append(int(row[2]))

        fpsI=int(row[3])
        fps.append(fpsI)
        totalFps=totalFps+fpsI
                 
        latI=int(row[4])
        latency.append(latI)
        totalLatency=totalLatency+latI

    time=len(sent)
    averageSent=totalSent/time
    averageRecived=totalRecived/time
    averageLatency=totalLatency/time
    averageFps=totalFps/time
    
        
    data=DataSet(os.path.splitext(os.path.basename(path))[0],seconds,read,sent,objUpdates,fps,latency,averageSent,averageRecived,averageLatency,averageFps);
    return data


#log the folder the graph data is being created from
print("\n\nCreating graph for: "+os.getcwd()+"/"+sys.argv[1]+"\n\n")

#get the working directory to load the data sets from
workingDir=os.getcwd()+"/"+sys.argv[1];


#load the data sets from the working directory
TestDataSets=[]
for root, dirs, files in os.walk(workingDir):
    for f in files:
        if os.path.splitext(f)[1] == '.txt':
            fullpath = os.path.join(root, f)
            print("Loading :"+f)
            TestDataSets.append(LoadDataFromFile(fullpath))

#Create a figure to create the graphs on
fig = plt.figure()

#create the sub plots for each of the logged data types
BytesSentGraph = fig.add_subplot(321,label = "Bytes Sent",ylabel='Bytes')
ComparisonBar = fig.add_subplot(322,label = "Average Sent And Recived",ylabel='Bytes')


BytesReadGraph = fig.add_subplot(323,sharex = BytesSentGraph, sharey = BytesSentGraph,label = "Bytes Read",ylabel='Bytes')
LatencyGraph = fig.add_subplot(324,label = "Latency",ylabel='ms')

UpdatesGraph = fig.add_subplot(325,sharex = BytesSentGraph,label = "Object Updates Recived",ylabel='updates')
FPSGraph = fig.add_subplot(326,label = "Average FPS",ylabel='Frames')


#plot the bytes sent data
BytesSentGraph.set_title('Bytes Sent')

for data in TestDataSets:
  BytesSentGraph.plot(data.seconds, data.sent, label = data.name)

#BytesSentGraph.get_xaxis().set_visible(False)
BytesSentGraph.legend(loc='upper center', bbox_to_anchor=(0.2, 1.35),fancybox=True, shadow=True, ncol=5)

#plot the bytes read data
BytesReadGraph.set_title('Bytes Recived')
for data in TestDataSets:
  BytesReadGraph.plot(data.seconds, data.read, label = data.name)

#BytesReadGraph.get_xaxis().set_visible(False)


##get average data for bar charts
DataSetCount=len(TestDataSets);
AverageSent= []
AverageRecived= []
AverageLatency= []
AverageFPS= []
groupNames=[]
for data in TestDataSets:
  groupNames.append(data.name)
  print(data.name)
  AverageSent.append(data.averageSent)
  AverageRecived.append(data.averageRecived)
  AverageLatency.append(data.averageLatency)
  AverageFPS.append(data.averageFps)

#plot the average bytes send and recived data for comparison
indent = np.arange(DataSetCount)
Barwidth=0.4

ComparisonBar.bar(indent, AverageSent, Barwidth, edgecolor="w", linewidth=3,label="Total Sent",color=['lightblue'])

ComparisonBar.bar(indent+Barwidth, AverageRecived, Barwidth, edgecolor="w",linewidth=3,label="Total Received",color=['purple'])


ComparisonBar.set_xticks(indent)
ComparisonBar.set_xticklabels(groupNames)

ComparisonBar.set_title('Average Sent & Received p/s')
ComparisonBar.legend(loc='upper center')

#--- plot the object updates
UpdatesGraph.set_title('Object Updates recived')
for data in TestDataSets:
  UpdatesGraph.plot(data.seconds, data.objUpdates, label = data.name)
  UpdatesGraph.set_xlabel('Seconds')


#--- plot the fps 
indent = np.arange(DataSetCount)/2
Barwidth=0.4

FPSGraph.bar(indent, AverageFPS, Barwidth, edgecolor="w", linewidth=3,label="FPS")

FPSGraph.set_xticks(indent)
FPSGraph.set_xticklabels(groupNames)

FPSGraph.set_title('Average FPS')

#--- plot the latency
LatencyGraph.set_title('Average Latency')
indent = np.arange(DataSetCount)/2
Barwidth=0.4

LatencyGraph.bar(indent, AverageLatency, Barwidth, edgecolor="w", linewidth=3,label="FPS")

LatencyGraph.set_xticks(indent)
LatencyGraph.set_xticklabels(groupNames)


#LatencyGraph.get_xaxis().set_visible(False)

#--adjust spacing
plt.subplots_adjust(left=0.1,
                    bottom=0.1, 
                    right=0.9, 
                    top=0.9, 
                    wspace=0.4, 
                    hspace=0.4)




#print read averages
print("------Read-------")
for data in TestDataSets:
  totalread=0
  for r in data.read:
    totalread=totalread+r

  average=totalread/len(data.read)
  print(data.name+" Average Bytes Read Per Second: "+str(average))
  
#print write averages
print("------write-------")
for data in TestDataSets:
  totalwrite=0
  for r in data.sent:
    totalwrite=totalwrite+r

  average=totalwrite/len(data.sent)
  print(data.name+" Average Bytes Sent Per Second: "+str(average))
  
#print latency averages
print("------Latency-------")
for data in TestDataSets:
  totalLatency=0
  for r in data.latency:
    totalLatency=totalLatency+r

  average=totalLatency/len(data.latency)
  print(data.name+" Average Latency Per Second: "+str(average))

#print fps averages
print("------fps-------")
for data in TestDataSets:
  totalFps=0
  for r in data.fps:
    totalFps=totalFps+r

  average=totalFps/len(data.fps)
  print(data.name+" Average FPS Per Second: "+str(average))

#print object averages
print("------objUpdates-------")
for data in TestDataSets:
  totalObjUpdates=0
  for r in data.objUpdates:
    totalObjUpdates=totalObjUpdates+r

  average=totalObjUpdates/len(data.objUpdates)
  print(data.name+" Average Object updates Per Second: "+str(average))


plt.show()
