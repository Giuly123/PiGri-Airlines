using MG.GIF;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedRawImage : MonoBehaviour
{
    private List<Texture2D> mFrames;
    private List<float>     mFrameDelay = new List<float>();

    private int   mCurFrame = 0;
    private float mTime     = 0.0f;

    [SerializeField]
    private RawImage RawImage;

    [SerializeField]
    private string Filename;

    private static Dictionary<string, GifFrames> cachedGif = new Dictionary<string, GifFrames>();

    class GifFrames{
        public List<float> mFrameDelay;
        public List<Texture2D> mFrames;
    }

    void Awake()
    {
        if( string.IsNullOrWhiteSpace( Filename ) )
        {
            return;
        }

        string path = Path.Combine( Application.streamingAssetsPath, Filename );

        GifFrames gifFrames = null;

        cachedGif.TryGetValue(path, out gifFrames);

        if(gifFrames == null)
        {
            mFrames = new List<Texture2D>();

            using (Decoder decoder = new Decoder(File.ReadAllBytes(path)))
            {
                MG.GIF.Image img = decoder.NextImage();

                while (img != null)
                {
                    mFrames.Add(img.CreateTexture());
                    mFrameDelay.Add(img.Delay / 1000.0f);
                    img = decoder.NextImage();
                }

                gifFrames = new GifFrames();

                gifFrames.mFrameDelay = mFrameDelay;
                gifFrames.mFrames = mFrames;

                cachedGif.Add(path, gifFrames);
            }
        }
        else
        {
            mFrameDelay = gifFrames.mFrameDelay;
            mFrames = gifFrames.mFrames;
        }

        RawImage.texture = mFrames[0];
    }

    void Update()
    {
        if( mFrames == null )
        {
            return;
        }

        mTime += Time.unscaledDeltaTime;

        if( mTime >= mFrameDelay[ mCurFrame ] )
        {
            mCurFrame = ( mCurFrame + 1 ) % mFrames.Count;
            mTime = 0.0f;

            RawImage.texture = mFrames[mCurFrame];
        }
    }
}

