using System;
using System.Collections;
using Core.GraphicPanel;
using UnityEngine;
using Core.ScriptIO;
using UnityEngine.Video;

namespace Core.CommandDB
{
    public class CMD_DatabaseExtensionGraphicPanel : CMD_DatabaseExtensionBase
    {
        private static string[] PARAM_PANEL_NAME = new string[] { "-p", "-panel" };
        private static string[] PARAM_LAYER = new string[] { "-l", "-layer" };
        private static string[] PARAM_MEDIA_NAME = new string[] { "-m", "-media" };
        private static string[] PARAM_SPEED = new string[] { "-spd", "-speed" };
        private static string[] PARAM_IMMEDIATE = new string[] { "-i", "-immediate" };
        private static string[] PARAM_BLEND_TEXTURE_NAME = new string[] { "-b", "-blend" };
        private static string[] PARAM_USEAUDIO_ON_VIDEO = new string[] { "-aud", "-audio" };

        private const string HOME_DIRECTORY_SYMBOL = "~/";

        new public static void Extend(CommandDatabase commandDatabase)
        {
            commandDatabase.AddCommand("setLayerMedia", new Func<string[], IEnumerator>(SetLayerMedia));
        }

        private static IEnumerator SetLayerMedia(string[] data)
        {
            var parameterFetcher = CreateFetcher(data);
            parameterFetcher.TryGetValue(PARAM_PANEL_NAME, out string panelName, "");
            parameterFetcher.TryGetValue(PARAM_LAYER, out int layer, 0);
            parameterFetcher.TryGetValue(PARAM_MEDIA_NAME, out string mediaName, "");
            parameterFetcher.TryGetValue(PARAM_IMMEDIATE, out bool immediate, false);
            parameterFetcher.TryGetValue(PARAM_SPEED, out float transitionSpeed, 1);
            parameterFetcher.TryGetValue(PARAM_BLEND_TEXTURE_NAME, out string blendTextureName, "");
            parameterFetcher.TryGetValue(PARAM_USEAUDIO_ON_VIDEO, out bool useAudioOnVideo, false);

            var panel = GraphicPanelManager.instance.FetchPanel(panelName);
            if (panel == null)
            {
                Debug.LogWarning($"Panel {panelName} not found");
                yield break;
            }

            string graphicPath = GetGraphicPath(UnityRuntimePathToolBox.ResourcesBackgroundImagePath, mediaName);
            UnityEngine.Object graphic = Resources.Load<Texture>(graphicPath);
            if (graphic == null)
            {
                graphicPath = GetGraphicPath(UnityRuntimePathToolBox.ResourcesBackgroundVideoPath, mediaName);
                graphic = Resources.Load<VideoClip>(graphicPath);
            }

            if (graphic == null)
            {
                Debug.LogError($"Could not find media file");
                yield break;
            }

            Texture blendTexture = null;
            if (!immediate && blendTextureName != string.Empty)
                blendTexture =
                    Resources.Load<Texture>(UnityRuntimePathToolBox.ResourcesBlendTexturePath + blendTextureName);

            GraphicLayer graphicLayer = panel.GetLayer(layer, true);
            if (graphic is Texture)
            {
                yield return graphicLayer.SetTexture(graphic as Texture, transitionSpeed, blendTexture, graphicPath,
                    immediate);
            }

            if (graphic is VideoClip)
            {
                yield return graphicLayer.SetVideo(graphic as VideoClip, transitionSpeed, useAudioOnVideo, blendTexture,
                    graphicPath, immediate);
            }
        }

        private static string GetGraphicPath(string defaultPath, string graphicName)
        {
            if (graphicName.StartsWith(HOME_DIRECTORY_SYMBOL))
                return graphicName[HOME_DIRECTORY_SYMBOL.Length..];
            return defaultPath + graphicName;
        }
    }
}