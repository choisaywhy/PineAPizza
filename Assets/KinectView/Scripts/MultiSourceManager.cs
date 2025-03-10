/* ***************************************************************
 * 프로그램 명 : MultiSourceManager.cs
 * 작성자 : 신은지, 최세화 (류서현, 이송이, 최은정, 홍예지) + 오픈소스 - kinect for unity
 * 최조 작성일 : 2019년 11월 14일
 * 최종 작성일 : 2019년 12월 15일
 * 프로그램 설명 : color, depth, infrared 기능들을 전부 합쳐와 전체적인 키넥트 인식 기능을 수행하도록 한다.
 * *************************************************************** */
using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class MultiSourceManager : MonoBehaviour {
    public int ColorWidth { get; private set; }
    public int ColorHeight { get; private set; }
    
    private KinectSensor _Sensor;
    private MultiSourceFrameReader _Reader;
    private Texture2D _ColorTexture;
    private ushort[] _DepthData;
    private byte[] _ColorData;

    public Texture2D GetColorTexture()
    {
        return _ColorTexture;
    }
    
    public ushort[] GetDepthData()
    {
        return _DepthData;
    }

    void Start () 
    {
        _Sensor = KinectSensor.GetDefault();
        
        if (_Sensor != null) 
        {
            _Reader = _Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);
            
            var colorFrameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            ColorWidth = colorFrameDesc.Width;
            ColorHeight = colorFrameDesc.Height;
            
            _ColorTexture = new Texture2D(colorFrameDesc.Width, colorFrameDesc.Height, TextureFormat.RGBA32, false);
            _ColorData = new byte[colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];
            
            var depthFrameDesc = _Sensor.DepthFrameSource.FrameDescription;
            _DepthData = new ushort[depthFrameDesc.LengthInPixels];
            
            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }
    
    void Update () 
    {
        if (_Reader != null) 
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                var colorFrame = frame.ColorFrameReference.AcquireFrame();
                if (colorFrame != null)
                {
                    var depthFrame = frame.DepthFrameReference.AcquireFrame();
                    if (depthFrame != null)
                    {
                        colorFrame.CopyConvertedFrameDataToArray(_ColorData, ColorImageFormat.Rgba);
                        _ColorTexture.LoadRawTextureData(_ColorData);
                        _ColorTexture.Apply();
                        
                        depthFrame.CopyFrameDataToArray(_DepthData);
                        
                        depthFrame.Dispose();
                        depthFrame = null;
                    }
                
                    colorFrame.Dispose();
                    colorFrame = null;
                }
                
                frame = null;
            }
        }
    }
    
    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }
        
        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }
            
            _Sensor = null;
        }
    }
}
