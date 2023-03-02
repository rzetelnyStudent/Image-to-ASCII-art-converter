# Image-to-ASCII-art-converter
Windows desktop image to ASCII art converter

This converter enables you to convert your favourite images to ascii art. Ascii art is a text file, that looks like a picture when zoomed-out. But when zoomed-in you can see that there are in fact letters that form a picture.
![image](https://user-images.githubusercontent.com/72305802/222418042-b7866bee-5496-44b5-9592-85dd2ab79192.png)
![image](https://user-images.githubusercontent.com/72305802/222418109-cc17e0f6-bb4b-4f08-aef8-1eb203ddd79f.png)
Generated picture-text file can be saved as .txt or .html.

This project enables to do some experiments on programming languages speed. You choose whether an image conversion should be done in C# or assembly code. For both ways you can pick the number of threads from 1 - 64, that conversion should use. The app measures time that the conversion has taken. There is even a option to export results to .csv file for better time relationship analysis:
![image](https://user-images.githubusercontent.com/72305802/222421151-9b86774e-b75c-44a9-9f23-f89584db8a4f.png)

Assembly code uses vector registers math (xmms) for even more parallelism.

