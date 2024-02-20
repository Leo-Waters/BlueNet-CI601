![](https://github.com/Leo-Waters/BlueNet-CI601/blob/main/Assets/BlueNet/Icons/BlueNetLogo.png?raw=true)

BlueNet is a bluetooth networking solution for unity; alowing developers to create multiplayer games via bluetooth instead of the traditional tcp/udp solutions for unity (UNET,Photon etc).

This Project was made for the university of brighton CI601(Final year project) by leo waters.

<h4>Platform Support</h4>

- Windows Network Transport utalizing 32feet.net to connect, send and recive data.

- Android Network Transport using JNI to interface with the included Android Studio Project.

<h4>Default Functionality</h4>

- The Network Manager component is the core of your project, as it is responsable to propergating network messages in and out, while providing functions for starting and joining a session, managing the game state, switching levels and providing events to let your game know of network changes.
  
- The Network Object component is used to simplify the synchronization of object specific data via sync vars and sending Remote procedure calls.
  
- The Network Transform Component provides synchronization of object transforms with perdiction for acurate object positioning.
  
- The Network Animator Component simplifies the synchronization of animation states by acting as a middle man for the default animator component.
  
<h4>Compression</h4>
The Compression systems can be chosen via editing the properties of the Network manager component; these compressions systems help to increase network throughput at the cost of performance.

<h5>Options</h5>

- None, sends the data as a string.
  
- Custom String Compressor, sends the data as a string, but uses a key value system to remove large re-occouring values.

- Gzip and Deflate, both algorithms compress the data before sending, and decompress the received data.

- Parallel Gzip and Deflate, essentialy the same as the non parellel versions, however the work load is split into chunks based on the size of the data being sent, the chunks are then compressed on seperate threads for faster proccessing times with the drawback of a lower compression ratio.

<h4>prerequisites</h4>
BlueNet Requires the 32 feet bluetooth package which gives access to bluetooth in c#, found here: https://github.com/inthehand/32feet;



