# LANPaint
LANPaint is a simple paint application which alows you to draw and send your drawing online to other people connected to the same network.

### Among the drawing features there are: 
* Choice between pencil and eraser 
* Tool thickness
* Pen color
* Background color
* Clear all the content
* Undoing and redoing drawing with CTRL+Z and CTRL+Y shortcuts
* Saving to a file and opening from a file your previously saved drawing (CTRL+S and CTRL+O)

### Among the network interaction features there are:
* Broadcast drawing, erasing, changing background, and clearing to other participants
* Synchronize (sending all your content to others)
* Turn off/on broadcast of your own drawing to others
* Turn off/on receive drawing from others
* Choose Network Adapter and port for interaction

![LANPaint_Owerview](https://user-images.githubusercontent.com/42912527/111908390-bea6df00-8a61-11eb-859a-30ff0a004de4.png)

# Network
Networking is just a UDP broadcasting, so it's not reliable but instead very simple to implement.

![LANPaint_Network](https://user-images.githubusercontent.com/42912527/111908870-8accb900-8a63-11eb-8132-f8016c0cbf2c.png)
