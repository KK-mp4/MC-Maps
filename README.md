# Minecraft Maps
## Desktop software to read coordinate data from Minecraft screenshots with F3 debug screen
[Video presentation/tutorial](https://youtu.be/TcY-gX_8bkM)
<p align="center">
  <img src="https://user-images.githubusercontent.com/103208695/163653876-47bf6315-744f-4c3d-8410-47b5121e4888.png"/>
</p>

Written on C# Windows Forms App (.NET Framework) and optical character recognition with Tesseract.Net SDK.<br />
Special thanks to [Hayden Carpenter](https://codedecatur.org/tutorials/hayden-carpenter/minecraft-ocr-with-pytesseract) for MC font training data.

![Screenshot 2022-04-17 092129](https://user-images.githubusercontent.com/103208695/163704372-6908a005-d959-46b7-a1d0-d953057fe82f.png)



## How to use:

### Step 0.
Go to settings tab, you will need to fill this in depending on the image data you are going to process.
**X** and **Y** are image coordinates of the top left corner of coordinates on your screenshot. **H** is height of cropped area.
In binarization threshold you will need to choose brightness range of F3 text. It's usually from 224 to 224 but for compressed images can be wider: 175 to 250 for example.<br />
Do not forget to click *Save settings* button after changes.

<p align="center">
  <img src="https://user-images.githubusercontent.com/103208695/163757897-c1f83bfa-c2d5-4542-b694-346098488559.png"/>
</p>

### Step 1.
By pressing *File* you can choose to import folder with images into project, one image or batch processing. If you selected last go to [step 4](#step-4).

![image](https://user-images.githubusercontent.com/103208695/163704438-445879af-1caf-4317-ab83-25de29a2fb5a.png)

### Step 2.
After it gets loaded you can select an image and press *Crop and Binarize* button.
As the name states this button will first crop your image to only coordinates and then binaraze image so text becomes solid black and everything else solid white.
This is done using a threshold RGB value you wrote in settings tab.<br />
Make sure that only text is visible, tweak settings and redo previous steps if needed.

![image](https://user-images.githubusercontent.com/103208695/163654232-03b76e82-5713-432e-8766-dabf943e729d.png)

### Step 3.
Next step you select cropped image and press *Get text* button, it will read text and output it on your screen.<br />

### Step 4.
After you got all the needed coordinates you can choose to go to map tab and import map image yourself and enter its coordinate range.<br />
**This is a plug for now, before I make map renderer myself or use some library.**

![image](https://user-images.githubusercontent.com/103208695/163760342-f7ae76bf-7efe-4203-8ca9-f67cf62e8f49.png)

Program will render markers if they exist in a given coordinate region. You can press them to display image taken in that spot.

### Step 5.
You can now press *Export marker data* button and it will generate marker_data.txt file in your project folder with image paths and their coordinates.

![image](https://user-images.githubusercontent.com/103208695/163758727-241ff510-411a-4661-a11a-bf07f9a2730e.png)
![image](https://user-images.githubusercontent.com/103208695/163759000-7ec35821-ca05-4a5f-8073-2a36f31c76b3.png)



## Performance

I ran some tests on 1.5k FullHD images on my laptop (PNG sequence of Dugged tour №6)
| Implementation  | Time | images/s | Ratio |
| ------------- | ------------- | ------------- | ------------- |
| Single thread  | 300026 ms | 5 | 100%  |
| Parallel processing | 171276 ms  | 8.75 | 57.09%  |

## License
This program is licensed under the MIT License. Please read the License file to know about the usage terms and conditions.


