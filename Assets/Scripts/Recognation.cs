using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Drawing;
using Point = OpenCvSharp.Point;
using System.Linq;
using Size = OpenCvSharp.Size;
using OpenCvSharp.Demo;
using TMPro;
using UnityEngine.UI;
using System.Diagnostics;

public class Recognation : MonoBehaviour
{

    private const KeyCode _oneFingerTapKeyCode = (KeyCode)330;
    private const KeyCode _twoFingerHoldKeyCode = (KeyCode)278;

    public TMP_Text text;
    public TMP_Text textPCB;
    public TMP_Text polarityText;
    public TMP_Text positionText;
    public TMP_Text resistanceCapacity;
    public GameObject planeImage;
    public WebCamTexture camTexture = null;
    public Texture2D imageTex;

    public int a;
  
    public Toggle toggle;
    public bool flag = true;
    Point2f[] sceneCornersGlob;
    Point2f[] sceneCornersGlobP;

    PCB pCB;

    KeyPoint[] keypointsImage;
    KeyPoint[] keypointsImageP;

    Mat descriptorsImage = new Mat();
    Mat descriptorsImagePart = new Mat();/// <summary>
    /// /////////////
    /// </summary>
    Mat descriptors = new Mat();

    Mat H = new Mat();
    Mat homoG = new Mat();
    Mat HP = new Mat();
    Mat homoGP = new Mat();
    ORB orb = ORB.Create(900);

    Point2f[] objCornersIMG = new Point2f[4];
    Point2f[] objCornersIMGPart = new Point2f[4];

    DescriptorMatcher flann = new FlannBasedMatcher();
    Mat img;//pcb
    CLAHE cLAHE = Cv2.CreateCLAHE(2.0, new Size(8,8));

    private int interval = 5;

    int counter = 0;
    
    public GameObject rawImageMenu;

    void Start()
    {
        
        
        
        img = OpenCvSharp.Unity.TextureToMat(imageTex);

        ImageORB(img, descriptorsImage, ref keypointsImage);
  
        GetCorners(img, objCornersIMG);
        
        camTexture = new WebCamTexture();
        planeImage.GetComponent<Renderer>().material.mainTexture = camTexture;
        
        camTexture.Play();

        pCB = new PCB("Infrared security barrier transmitter", "surface mounting", new Assets.Resistor("Resistor","220 Ohm", "R2", 1),
            new Assets.Ñapacitor("Capacitor", "10 pF", "C4", 1), new Assets.Diod("Diod", 1, "D2"));
        toggle.isOn = false;
    }

    
    // Update is called once per frame
    void Update()
    {
        if (rawImageMenu.activeSelf == false)
        {


            Mat frameDisplay = OpenCvSharp.Unity.TextureToMat(camTexture);

           

            var red_lower = new OpenCvSharp.Scalar(0, 52, 0);
            var red_upper = new OpenCvSharp.Scalar(179, 255, 255);
            //
            var yellow_lower = new OpenCvSharp.Scalar(0, 88, 91);
            var yellow_upper = new OpenCvSharp.Scalar(150, 255, 170);
            //
            var blue_lower = new OpenCvSharp.Scalar(0, 0, 60);
            var blue_upper = new OpenCvSharp.Scalar(179, 255, 120);


            if (flag)
            {
                StartCoroutine(ExampleCoroutine());
            }


            if (flag == false)
            {

                if (Input.GetKeyDown(_oneFingerTapKeyCode))
                {
                    counter++;
                    UnCheckDetail();
                }
                if (counter == 1)
                {
                   
                    PCBInformation(pCB);
                   
                    Orb(frameDisplay, objCornersIMG, descriptorsImage, H, homoG, keypointsImage, ref sceneCornersGlob, 0);
                }
                if (counter == 2 || counter == 4 || counter == 6 || counter == 8)
                {
                    text.text = "";
                    textPCB.text = "";
                    polarityText.text = "";
                    positionText.text = "";
                    resistanceCapacity.text = "";
                    toggle.isOn = false;
                    Texture tex = OpenCvSharp.Unity.MatToTexture(frameDisplay);
                    planeImage.GetComponent<Renderer>().material.mainTexture = tex;
                }
                if (counter == 3)
                {
                    
                    DetailInformation(pCB.Diod.Name, pCB.Diod.Count);
                    GetContours(frameDisplay, red_lower, red_upper);
                    if (toggle.isOn == true)
                    {
                        Position(pCB.Diod.Position);
                        Polarity(pCB.Diod.Polarity);
                    }
                }
                if (counter == 5)
                {
                    DetailInformation(pCB.Resistor.Name, pCB.Resistor.Count);
                    GetContours(frameDisplay, blue_lower, blue_upper);
                    if (toggle.isOn == true)
                    {
                        
                        ResistanceCapacity(pCB.Resistor.Resistance);
                        Position(pCB.Resistor.Position);
                        Polarity(pCB.Resistor.Polarity);
                    }
                }
                if (counter == 7)
                {
                    DetailInformation(pCB.Capacitor.Name, pCB.Capacitor.Count);
                    
                    GetContours(frameDisplay, yellow_lower, yellow_upper);
                    if (toggle.isOn == true)
                    {
                        ResistanceCapacity(pCB.Capacitor.Capacity);
                        Position(pCB.Capacitor.Position);
                        Polarity(pCB.Capacitor.Polarity);
                    }
                }
            }
        }

        if (Input.GetKeyDown(_twoFingerHoldKeyCode))
        {
            Application.Quit();
        }
    }



   

    IEnumerator ExampleCoroutine()
    {
        
        yield return new WaitForSeconds(2);
        flag = false;
        
    }

   
    private void ImageORB(Mat image, Mat desc, ref KeyPoint[] kp )
    {
        Cv2.CvtColor(image, image, ColorConversionCodes.RGB2GRAY);
        cLAHE.Apply(image, image);
        Cv2.Blur(image, image, new Size(3, 3), default);
        Mat mask1 = new Mat();
        var lower_green = new OpenCvSharp.Scalar(36, 0, 0);
        var upper_green = new OpenCvSharp.Scalar(86, 255, 255);

        Cv2.InRange(image, lower_green, upper_green, mask1);

        kp = orb.Detect(image); 
        orb.Compute(image, ref kp, desc);
    }

    private void Orb(Mat frameDisplay, Point2f[] corners, Mat descriptorsImg, Mat homogr, Mat newHomogr, KeyPoint[] kp, ref Point2f[] sCG, int index)
    {
      
        Mat frame = frameDisplay.Clone();
        Cv2.CvtColor(frame, frame, ColorConversionCodes.RGB2GRAY);
        cLAHE.Apply(frame, frame);
        Cv2.Blur(frame, frame, new Size(3, 3), default); 
        KeyPoint[] keypoints = orb.Detect(frame);
        
         
        
        orb.Compute(frame, ref keypoints, descriptors);
        descriptors.ConvertTo(descriptors, MatType.CV_32F);
        descriptorsImg.ConvertTo(descriptorsImg, MatType.CV_32F);

        
        var dMatches = flann.KnnMatch(descriptorsImg, descriptors,  2);
        
        List<DMatch> goodMatches = RatioTest(dMatches);
       
        
        

        Point2d[] obj = new Point2d[goodMatches.Count()];
        Point2d[] scene = new Point2d[goodMatches.Count()];

        for (int i = 0; i < goodMatches.Count(); i++)
        {
            obj[i] = new Point2d(kp[goodMatches[i].QueryIdx].Pt.X, kp[goodMatches[i].QueryIdx].Pt.Y);
            scene[i] = new Point2d(keypoints[goodMatches[i].TrainIdx].Pt.X, keypoints[goodMatches[i].TrainIdx].Pt.Y);
        }


       
        homogr = Cv2.FindHomography(obj, scene, HomographyMethods.Ransac, 3, null);
        
        newHomogr = Cv2.FindHomography(obj, scene, HomographyMethods.Ransac, 3, null);

        if (homogr.Empty()==true )
        {
            newHomogr.CopyTo(homogr); 

        }


        bool getlines = false;

        if (homogr.Empty()==false && goodMatches.Count() > 20)
        {
            Cv2.AccumulateWeighted(newHomogr, homogr, 0.09, null);
            Point2f[] sC = Cv2.PerspectiveTransform(corners, homogr);
           

            
            if (sC.Length != 0 )
            {
                if ((sC[3].X - sC[0].X) > frame.Width / 7 )
                {
                //    Point p1 = new Point(sC[0].X, sC[0].Y);
                //    Point p2 = new Point(sC[3].X, sC[3].Y);

                //    //Cv2.Rectangle(frameDisplay, p1, p2, new Scalar(0, 255, 0), 20);
                //    Cv2.Line(frameDisplay, p1, p2, new Scalar(0, 255, 0), 20);
                //    p1 = new Point(sC[1].X, sC[1].Y);
                //    p2 = new Point(sC[2].X, sC[2].Y);
                //    Cv2.Line(frameDisplay, p1, p2, new Scalar(0, 255, 0), 20);
                //    p1 = new Point(sC[2].X, sC[2].Y);
                //    p2 = new Point(sC[3].X, sC[3].Y);
                //    Cv2.Line(frameDisplay, p1, p2, new Scalar(0, 255, 0), 20);
                //    p1 = new Point(sC[3].X, sC[3].Y);
                //    p2 = new Point(sC[0].X, sC[0].Y);
                //    Cv2.Line(frameDisplay, p1, p2, new Scalar(0, 255, 0), 20);
                    sCG = sC;



                    CheckDetail();

                    
                    
                }

            }
            getlines = true;

        }

        
        homogr.Release();

        Texture tex = OpenCvSharp.Unity.MatToTexture(frameDisplay);
        planeImage.GetComponent<Renderer>().material.mainTexture = tex;

    }

    public void GetCorners(Mat image, Point2f[] c)
    {
        c[0] = new Point(0f, 0f);
        c[1] = new Point((float)image.Cols, 0f);
        c[2] = new Point((float)image.Cols, (float)image.Rows);
        c[3] = new Point(0f, (float)image.Rows);
    }

   
    public List<DMatch> RatioTest(DMatch[][] matches)
    {
        List<DMatch> goodMatches = new List<OpenCvSharp.DMatch>();

        for (int i = 0; i < matches.Length; i++)
        {
            if (matches[i][0].Distance < 0.8 * matches[i][1].Distance)
            {
                goodMatches.Add(matches[i][0]);
            }
        }

        return goodMatches;
    }
    

    public void Polarity(bool polarity)
    {
        if (polarity)
        {
            polarityText.text = "Check the polarity before";
        }
        else
        {
            polarityText.text = "Non - polar";
        }
        
    }

    public void ResistanceCapacity(string resCap)
    {
        resistanceCapacity.text = resCap;
    }

    public void Position(string pos)
    {
        positionText.text = "Position: "+pos;
    }


    public void DetailInformation(string name, int count)
    {
        text.text = name + "  count: "+ count.ToString();
        

    }

    public void PCBInformation(PCB pCB)
    {
        pCB.Detected = true;
        polarityText.text = "";
        textPCB.text = pCB.Name;
    }

    public void CheckDetail()
    {
        toggle.isOn = true;
    }

    public void UnCheckDetail()
    {
        toggle.isOn = false;
    }

    public void GetContours(Mat frameDisplay,  Scalar lower, Scalar upper)
    {
        Mat frame = frameDisplay.Clone();

        Cv2.CvtColor(frame, frame, ColorConversionCodes.BGR2HSV);

        //
       
        //
        Mat mask1 = new Mat();
        
        Mat res = new Mat();

        byte[] kernelValues = { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        Mat kernel = new Mat(3, 3, MatType.CV_8UC1, kernelValues);

        Cv2.InRange(frame, lower, upper, mask1);
        
        Cv2.Dilate(mask1, mask1, kernel);
       
        Cv2.BitwiseAnd(frame, frame, res, mask1);

        Point[][] contours; 
        HierarchyIndex[] hierarchyIndexes;

        Cv2.FindContours(mask1, out contours, out hierarchyIndexes, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

        if (contours.Length>= 1)
        {
            var biggestContourRect = Cv2.BoundingRect(contours[0]);
            if((biggestContourRect.Width )> 7)
            {
                Cv2.Rectangle(frameDisplay,
                new OpenCvSharp.Point(biggestContourRect.X, biggestContourRect.Y),
                new OpenCvSharp.Point(biggestContourRect.X + biggestContourRect.Width, biggestContourRect.Y + biggestContourRect.Height),
                new Scalar(0, 50, 255), 2);

                CheckDetail();
            }
            

        }

        //var br = Cv2.BoundingRect(contours[0]);
        //Cv2.Rectangle(frameDisplay,
        //    new OpenCvSharp.Point(br.X, br.Y),
        //    new OpenCvSharp.Point(br.X + br.Width, br.Y + br.Height),
        //    new Scalar(0, 50, 255), 2);

     
        
        Texture tex = OpenCvSharp.Unity.MatToTexture(frameDisplay);
        planeImage.GetComponent<Renderer>().material.mainTexture = tex;

    }

}
