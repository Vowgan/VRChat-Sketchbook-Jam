
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VowganVR
{
    public class Screenshotter : EditorWindow
    {
        
        // Date  5-22-2021
        // Build v0.3
        
        private bool transparent;
        private bool autoOpen = true;
        private string folderPath = "";
        private string lastImage = "";
        
        private string[] sFormat =
        {
            "SceneName Hour-Minute-Second",
            "SceneName Month-Date-Year Hour-Minute-Second",
            "Month-Date-Year Hour-Minute-Second",
            "CustomName Hour-Minute-Second"
        };
        
        private Vector2[] presets =
        {
            new Vector2(1920, 1080),
            new Vector2(2560, 1440),
            new Vector2(3840, 2160),
            new Vector2(7680, 4320),
            new Vector2(320, 240),
            new Vector2(640, 480),
            new Vector2(800, 600),
            new Vector2(1024, 768),
            new Vector2(1280, 960),
            new Vector2(1536, 1180),
            new Vector2(1600, 1200),
            new Vector2(2048, 1536),
            new Vector2(2240, 1680),
            new Vector2(2560, 1920),
            new Vector2(3032, 2008),
            new Vector2(3072, 2304),
            new Vector2(3264, 2448),
        };

        private string[] presetsString =
        {
            "1920, 1080",
            "2560, 1440",
            "3840, 2160",
            "7680, 4320",
            "320, 240",
            "640, 480",
            "800, 600",
            "1024, 768",
            "1280, 960",
            "1536, 1180",
            "1600, 1200",
            "2048, 1536",
            "2240, 1680",
            "2560, 1920",
            "3032, 2008",
            "3072, 2304",
            "3264, 2448"
        };

        private string customName = "Screenshot";
        private int sFormatIndex;
        private int presetsIndex;

        public Camera scCamera;
        private int imageWidth = 1920;
        private int imageHeight = 1080;

        private bool showCameraSelection = true;
        private bool showImageResolution = true;
        private bool showSaveImage = true;

        private GUIStyle iconStyle;


        [MenuItem("Tools/Vowgan/Screenshotter")]
        public static void ShowWindow()
        {
            EditorWindow win = GetWindow<Screenshotter>();
            win.minSize = new Vector2(230, 300);
            win.titleContent.image = EditorGUIUtility.ObjectContent(null, typeof(Camera)).image;
            win.titleContent.text = "Screenshotter";
            win.Show();
        }

        private void OnEnable()
        {
            iconStyle = new GUIStyle
            {
                normal =
                {
                    background = Resources.Load("VV Icon") as Texture2D,
                },
                fixedHeight = 64,
                fixedWidth = 64
            };
        }

        private void OnGUI()
        {
            DrawTitle();

            GUILayout.Space(5);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);
            CameraSelection();

            GUILayout.Space(5);

            ImageResolution();

            GUILayout.Space(5);

            SaveImage();
            GUILayout.Space(5);
            GUILayout.EndVertical();
        }
        
        private void DrawTitle()
        {
            GUILayout.BeginHorizontal();
            
            GUILayout.BeginVertical();
            GUILayout.Box("", iconStyle);
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical();
            
            GUILayout.Label("Screenshotter Settings", EditorStyles.boldLabel);
            GUILayout.Label("VowganVR");
            
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("Capture Image", GUILayout.MinHeight(60)))
            {
                if (folderPath == "")
                {
                    folderPath = EditorUtility.SaveFolderPanel("Image Save Location", folderPath, Application.dataPath);
                    RenderImage();
                }
                else
                {
                    RenderImage();
                }
            }

            GUILayout.Space(5);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Screenshot", EditorStyles.miniButtonLeft, GUILayout.Height(20)))
            {
                if (lastImage != "")
                {
                    Application.OpenURL("file://" + lastImage);
                }
                else
                {
                    Debug.Log("No previous screenshot.");
                }
            }

            if (GUILayout.Button("Open Folder", EditorStyles.miniButtonRight, GUILayout.Height(20)) && !string.IsNullOrEmpty(folderPath))
            {
                EditorUtility.RevealInFinder(folderPath);
            }

            GUILayout.EndHorizontal();
            
            GUILayout.Space(2);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(2);
            
        }
        
        private void CameraSelection()
        {
            showCameraSelection = EditorGUILayout.Foldout(showCameraSelection, "Select Camera", true);
            if (showCameraSelection)
            {
                GUILayout.BeginVertical("box");
                
                scCamera = (Camera) EditorGUILayout.ObjectField(scCamera, typeof(Camera), true);
    
                if (scCamera == null) scCamera = Camera.main;
    
                transparent = EditorGUILayout.ToggleLeft("Transparent", transparent);
                autoOpen = EditorGUILayout.ToggleLeft("Open Image", autoOpen);
                GUILayout.Space(2);
                GUILayout.EndVertical();
            }
        }

        private void ImageResolution()
        {
            showImageResolution = EditorGUILayout.Foldout(showImageResolution, "Resolution", true);
            if (showImageResolution)
            {
                GUILayout.BeginVertical("box");

                imageWidth = EditorGUILayout.IntField("Width", imageWidth);
                imageHeight = EditorGUILayout.IntField("Height", imageHeight);
                if (imageHeight <= 0) imageHeight = 1;
                if (imageWidth <= 0) imageWidth = 1;

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Use Screen Size", EditorStyles.miniButtonLeft,
                    GUILayout.Width(position.width / 2 - 6), GUILayout.Height(18)))
                {
                    imageWidth = (int) Handles.GetMainGameViewSize().x;
                    imageHeight = (int) Handles.GetMainGameViewSize().y;
                }

                var oldPreset = presetsIndex;
                presetsIndex = EditorGUILayout.Popup(presetsIndex, presetsString, EditorStyles.miniButtonRight, GUILayout.Height(18));
                if (oldPreset != presetsIndex)
                {
                    imageWidth = (int) presets[presetsIndex].x;
                    imageHeight = (int) presets[presetsIndex].y;
                }

                GUILayout.EndHorizontal();

                GUILayout.Space(4);
                GUILayout.EndVertical();
            }
        }

        private void SaveImage()
        {
            showSaveImage = EditorGUILayout.Foldout(showSaveImage, "Save Image", true);
            if (showSaveImage)
            {
                GUILayout.BeginVertical("box");

                GUILayout.BeginHorizontal();
                EditorGUILayout.TextField(folderPath);
                if (GUILayout.Button("Browse", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
                {
                    folderPath = EditorUtility.SaveFolderPanel("Path to Save Images", folderPath, Application.dataPath);
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(4);

                sFormatIndex = EditorGUILayout.Popup("Naming Format", sFormatIndex, sFormat);

                if (sFormatIndex == 3 ^ sFormatIndex == 4)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Custom Name");
                    customName = GUILayout.TextField(customName);
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(8);
                GUILayout.EndVertical();
            }
        }

        private void RenderImage()
        {
            var rt = new RenderTexture(imageWidth, imageHeight, 24);
            scCamera.targetTexture = rt;

            TextureFormat tFormat;

            if (transparent)
            {
                tFormat = TextureFormat.ARGB32;
            }
            else
            {
                tFormat = TextureFormat.RGB24;
            }


            var screenShot = new Texture2D(imageWidth, imageHeight, tFormat, false);

            if (transparent)
            {
                var nativeFlags = scCamera.clearFlags;
                scCamera.clearFlags = CameraClearFlags.Nothing;
                scCamera.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
                scCamera.targetTexture = null;
                RenderTexture.active = null;
                scCamera.clearFlags = nativeFlags;
            }
            else
            {
                scCamera.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
                scCamera.targetTexture = null;
                RenderTexture.active = null;
            }

            byte[] bytes = screenShot.EncodeToPNG();
            var filename = NameImage();

            System.IO.File.WriteAllBytes(filename, bytes);
            if (autoOpen) Application.OpenURL(filename);
        }
        
        private string NameImage()
        {
            string newName = "Screenshot";
            var scene = SceneManager.GetActiveScene();

            switch (sFormatIndex)
            {
                case 0:
                    newName = folderPath + "/" + scene.name + "_" + DateTime.Now.ToString("HH-mm-ss") + ".png";
                    break;
                case 1:
                    newName = folderPath + "/" + scene.name + "_" + DateTime.Now.ToString("MM-dd-yyyy'_'HH-mm-ss") + ".png";
                    break;
                case 2:
                    newName = folderPath + "/" + DateTime.Now.ToString("MM-dd-yyyy'_'HH-mm-ss") + ".png";
                    break;
                case 3:
                    newName = folderPath + "/" + customName + "_" + DateTime.Now.ToString("HH-mm-ss") + ".png";
                    break;
            }

            lastImage = newName;

            return newName;
        }

    }
}