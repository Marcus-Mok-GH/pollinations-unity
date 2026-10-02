using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Pollinations.Unity.Samples
{
    /// <summary>
    /// End-to-end demo: one prompt drives text, image, and speech generation.
    /// The Image and Status references are optional - when they are missing the
    /// component builds a minimal overlay UI at runtime, so the sample scene
    /// works with nothing assigned.
    ///
    /// The development API key is deliberately NOT a serialized field: Unity
    /// serializes public fields into scenes, prefabs, and builds. It is read
    /// at runtime from EditorPrefs (editor only, set via the Tools menu) or the
    /// POLLINATIONS_API_KEY environment variable, and it never leaves this
    /// process. Shipped games should use the PollinationsAuth device flow.
    /// </summary>
    public sealed class PollinationsDemo : MonoBehaviour
    {
        [Header("Prompt used for the image; the text call describes it in words.")]
        public string Prompt = "A tiny pixel-art garden on an alien planet";
        public string AppKey;

        public RawImage Image;
        public Text Status;
        public AudioSource Audio;

        /// <summary>In-memory key override; not serialized by Unity.</summary>
        public string ApiKey { get; set; }
        private PollinationsClient _client;

        private void Awake()
        {
            if (Audio == null)
            {
                Audio = GetComponent<AudioSource>();
                if (Audio == null)
                    Audio = gameObject.AddComponent<AudioSource>();
            }
            if (Image == null || Status == null)
                BuildDefaultUi();
            if (string.IsNullOrEmpty(ApiKey))
                ApiKey = ResolveApiKey();
            _client = new PollinationsClient { ApiKey = ApiKey };
        }

        [ContextMenu("Connect Pollen")]
        public async void ConnectPollen()
        {
            if (string.IsNullOrEmpty(AppKey))
            {
                SetStatus("Set a publishable App Key on this component first.");
                return;
            }

            try
            {
                var result = await new PollinationsAuth(AppKey).AuthorizeAsync(progress => SetStatus(progress.Message));
                if (result.State == DeviceFlowState.Granted)
                    _client.DeviceToken = result.AccessToken;
                SetStatus(result.State == DeviceFlowState.Granted ? "Connected. Run the demo." : "Connection " + result.State + ".");
            }
            catch (Exception exception)
            {
                SetStatus("Connection failed: " + exception.Message);
            }
        }

        [ContextMenu("Run Pollinations demo")]
        public async void Run()
        {
            if (string.IsNullOrEmpty(_client.DeviceToken) && string.IsNullOrEmpty(ApiKey))
            {
                SetStatus("Connect Pollen with an App Key, or set a development key in Tools > Pollinations.");
                return;
            }

            SetStatus("Generating text...");
            var text = await _client.TextAsync("Describe this scene in one cheerful sentence: " + Prompt);
            SetStatus(text.Text);

            SetStatus("Generating image...");
            var image = await _client.ImageAsync(Prompt, width: 512, height: 512);
            if (Image != null)
                Image.texture = image.Texture;

            SetStatus("Generating speech...");
            var speech = await _client.SpeechAsync(text.Text);
            if (Audio != null)
            {
                Audio.clip = speech.Clip;
                Audio.Play();
            }
            SetStatus("Done - the sentence was also spoken.");
        }

        private static string ResolveApiKey()
        {
#if UNITY_EDITOR
            var editorKey = UnityEditor.EditorPrefs.GetString("PollinationsApiKey");
            if (!string.IsNullOrEmpty(editorKey))
                return editorKey;
#endif
            return Environment.GetEnvironmentVariable("POLLINATIONS_API_KEY") ?? string.Empty;
        }

        private void BuildDefaultUi()
        {
            var canvasGo = new GameObject(
                "PollinationsCanvas",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);

            var imageGo = new GameObject("GeneratedImage", typeof(RawImage));
            imageGo.transform.SetParent(canvasGo.transform, false);
            var imageRect = (RectTransform)imageGo.transform;
            imageRect.anchorMin = imageRect.anchorMax = imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.anchoredPosition = Vector2.zero;
            imageRect.sizeDelta = new Vector2(512, 512);
            Image = imageGo.GetComponent<RawImage>();

            var statusGo = new GameObject("StatusText", typeof(Text));
            statusGo.transform.SetParent(canvasGo.transform, false);
            var statusRect = (RectTransform)statusGo.transform;
            statusRect.anchorMin = new Vector2(0, 0);
            statusRect.anchorMax = new Vector2(1, 0);
            statusRect.offsetMin = new Vector2(16, 12);
            statusRect.offsetMax = new Vector2(-16, 64);
            var status = statusGo.GetComponent<Text>();
            status.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (status.font == null)
                status.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            status.fontSize = 22;
            status.color = Color.white;
            status.alignment = TextAnchor.LowerLeft;
            Status = status;
        }

        private void SetStatus(string message)
        {
            if (Status != null)
                Status.text = message;
            Debug.Log("[Pollinations] " + message);
        }
    }
}
