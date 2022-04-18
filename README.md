# Minecraft Maps
## Desktop software to read coordinate data from Minecraft screenshots with F3 debug screen
<p align="center">
  <img src="https://user-images.githubusercontent.com/103208695/163653876-47bf6315-744f-4c3d-8410-47b5121e4888.png"/>
</p>

Written on C# Windows Forms App (.NET Framework) and optical character recognition with Tesseract.Net SDK.<br />
Special thanks to [Hayden Carpenter](https://codedecatur.org/tutorials/hayden-carpenter/minecraft-ocr-with-pytesseract) for MC font training data.

![Screenshot 2022-04-17 092129](https://user-images.githubusercontent.com/103208695/163704372-6908a005-d959-46b7-a1d0-d953057fe82f.png)

## How to use:

**Step 1.** By pressing *File* you can choose to import folder with images into project, one image or batch processing. If you selected last go to step 4.

![image](https://user-images.githubusercontent.com/103208695/163704438-445879af-1caf-4317-ab83-25de29a2fb5a.png)

**Step 2.** After it gets loaded you can select an image and press *Crop and Binarize* button.
As the name states this button will first crop your image to only coordinates and then binaraze image so text becomes solid black and everything else solid white.
This is done by having a treshold RGB value at 225, 225, 225.<br />

![image](https://user-images.githubusercontent.com/103208695/163654232-03b76e82-5713-432e-8766-dabf943e729d.png)

**Step 3.** Next step you select new image and press *Get text* button, it will read text and output it on your screen.<br />

**Step 4.** After you got all the needed coordinates you can choose to go to map tab and import map image yourself and enter its coordinate range.<br />
**This is a plug for now, before I make map renderer myself or use some library.**

![image](https://user-images.githubusercontent.com/103208695/163704699-1906a6c0-622c-435f-b336-a971ba2e9c80.png)

Program will render markers if they exist in a given coordinate region. You can press them to display image taken in that spot.

## Performance

I ran some tests on 1.5k FullHD images on my laptop (PNG sequence of Dugged tour №6)
| Implementation  | Time | images/s | Ratio |
| ------------- | ------------- | ------------- | ------------- |
| Single thread  | 300026 ms | 5 | 100%  |
| Parallel processing | 171276 ms  | 8.75 | 57.09%  |

## License
This program is licensed under the MIT License. Please read the License file to know about the usage terms and conditions.


