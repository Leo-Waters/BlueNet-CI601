from pathlib import Path
import matplotlib.pyplot as plt
import numpy as np
import sys
import os

#Data Set object representing the data of a compression test
class DataSet:
  def __init__(self,name,seconds,read, sent,objUpdates,fps,latency,totalSent,totalRecived):
    self.name=name
    self.seconds = seconds
    self.read = read
    self.sent = sent
    self.objUpdates = objUpdates
    self.fps = fps
    self.latency=latency
    self.totalRecived=totalRecived
    self.totalSent=totalSent

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
        fps.append(int(row[3]))
        latency.append(float(row[4]))

    
        
    data=DataSet(os.path.splitext(os.path.basename(path))[0],seconds,read,sent,objUpdates,fps,latency,totalSent,totalRecived);
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
ComparisonBar = fig.add_subplot(322,label = "Total Sent And Recived",ylabel='Bytes')


BytesReadGraph = fig.add_subplot(323,sharex = BytesSentGraph, sharey = BytesSentGraph,label = "Bytes Read",ylabel='Bytes')
LatencyGraph = fig.add_subplot(324,sharex = BytesSentGraph,label = "Latency",ylabel='ms')

UpdatesGraph = fig.add_subplot(325,sharex = BytesSentGraph,label = "Object Updates Recived",ylabel='updates')
FPSGraph = fig.add_subplot(326,sharex = BytesSentGraph,label = "FPS",ylabel='Frames')


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

#plot the total bytes send and recived data for comparison

DataSetCount=len(TestDataSets);
TotalSent= []
TotalRecived= []
groupNames=[]
for data in TestDataSets:
  groupNames.append(data.name)
  print(data.name)
  TotalSent.append(data.totalSent)
  TotalRecived.append(data.totalRecived)

indent = np.arange(DataSetCount)
Barwidth=0.4

ComparisonBar.bar(indent, TotalSent, Barwidth, edgecolor="w", linewidth=3,label="Total Sent",color=['lightblue'])

ComparisonBar.bar(indent+Barwidth, TotalRecived, Barwidth, edgecolor="w",linewidth=3,label="Total Received",color=['purple'])


ComparisonBar.set_xticks(indent)
ComparisonBar.set_xticklabels(groupNames)

ComparisonBar.set_title('Total Sent & Received')
ComparisonBar.legend(loc='upper center')

#--- plot the object updates
UpdatesGraph.set_title('Object Updates recived')
for data in TestDataSets:
  UpdatesGraph.plot(data.seconds, data.objUpdates, label = data.name)
  UpdatesGraph.set_xlabel('Seconds')


#--- plot the fps
FPSGraph.set_title('Frames per second')
for data in TestDataSets:
  FPSGraph.plot(data.seconds, data.fps, label = data.name)
FPSGraph.set_xlabel('Seconds')

#--- plot the latency
LatencyGraph.set_title('Latency')
for data in TestDataSets:
  LatencyGraph.plot(data.seconds, data.latency, label = data.name)

#LatencyGraph.get_xaxis().set_visible(False)

#--adjust spacing
plt.subplots_adjust(left=0.1,
                    bottom=0.1, 
                    right=0.9, 
                    top=0.9, 
                    wspace=0.4, 
                    hspace=0.4)




#print averages
for data in TestDataSets:
  totalread=0
  for r in data.read:
    totalread=totalread+r

  average=totalread/len(data.read)
  print(data.name+" Average Bytes Sent Per Second: "+str(average))

plt.show()
