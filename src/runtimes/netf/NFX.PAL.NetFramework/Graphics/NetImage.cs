﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;

using NFX.Graphics;
using NFXImageFormat=NFX.Graphics.ImageFormat;
using NFX.PAL.Graphics;

namespace NFX.PAL.NetFramework.Graphics
{
  /// <summary>
  /// Implements image using .NET framework GDI+ wrapper
  /// </summary>
  public sealed class NetImage : DisposableObject, IPALImage
  {

    internal NetImage(Size size, Size resolution, ImagePixelFormat pixFormat)
    {
      var pf = xlat(pixFormat);
      m_Bitmap = new Bitmap(size.Width, size.Height, pf);
      m_Bitmap.SetResolution(resolution.Width, resolution.Height);
    }

    internal NetImage(System.Drawing.Image img)
    {
      var bmp = img as Bitmap;
      if (bmp==null)
        throw new NetFrameworkPALException(StringConsts.ARGUMENT_ERROR + $"{nameof(NetImage)}.ctor(img!bitmap)");
      m_Bitmap = bmp;
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Bitmap);
    }

    private Bitmap m_Bitmap;

    internal Bitmap Bitmap => m_Bitmap;

    public ImagePixelFormat PixelFormat => xlat(m_Bitmap.PixelFormat);

    public Color GetPixel(Point p) => m_Bitmap.GetPixel(p.X, p.Y);
    public void SetPixel(Point p, Color color) => m_Bitmap.SetPixel(p.X, p.Y, color);

    public Color GetPixel(PointF p) => m_Bitmap.GetPixel((int)p.X, (int)p.Y);
    public void SetPixel(PointF p, Color color) => m_Bitmap.SetPixel((int)p.X, (int)p.Y, color);

    public Size GetResolution() => new Size((int)m_Bitmap.HorizontalResolution, (int)m_Bitmap.VerticalResolution);
    public void SetResolution(Size resolution) => m_Bitmap.SetResolution(resolution.Width, resolution.Height);

    public Size GetSize() => m_Bitmap.Size;


    public void MakeTransparent(Color? dflt)
    {
      if (dflt.HasValue)
        m_Bitmap.MakeTransparent(dflt.Value);
      else
        m_Bitmap.MakeTransparent();
    }

    public IPALCanvas CreateCanvas()
    {
      var ngr = System.Drawing.Graphics.FromImage(m_Bitmap);
      return new NetCanvas(ngr);
    }

    public void Save(string fileName, NFXImageFormat format)
    {
      var (codec, pars) = getEncoder(format);
      m_Bitmap.Save(fileName, codec, pars);
    }

    public void Save(Stream stream, NFXImageFormat format)
    {
      var (codec, pars) = getEncoder(format);
      m_Bitmap.Save(stream, codec, pars);
    }

    public byte[] Save(NFXImageFormat format)
    {
      using(var ms = new MemoryStream())
      {
        this.Save(ms, format);
        return ms.ToArray();
      }
    }

    private (ImageCodecInfo codec, EncoderParameters pars) getEncoder(NFXImageFormat format)
    {
      ImageCodecInfo codec;
      EncoderParameters pars;

      if (format is BitmapImageFormat)
      {
        codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Bmp.Guid);
        pars = null;//new EncoderParameters(0);
      } else if (format is PngImageFormat)
      {
        codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Png.Guid);
        pars = null;//new EncoderParameters(0);
      } else if (format is GifImageFormat)
      {
        codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Gif.Guid);
        pars = null;//new EncoderParameters(0);
      } else//default is JPEG
      {
        var jpeg = format as JpegImageFormat;
        codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
        pars = new EncoderParameters(1);
        pars.Param[0] = new EncoderParameter(Encoder.Quality, jpeg?.Quality ?? 80L);
      }


      return ( codec: codec, pars: null );
    }

    private static System.Drawing.Imaging.PixelFormat xlat(ImagePixelFormat pf)
    {
      switch(pf)
      {
        case ImagePixelFormat.BPP1Indexed: return System.Drawing.Imaging.PixelFormat.Format1bppIndexed;
        case ImagePixelFormat.BPP4Indexed: return System.Drawing.Imaging.PixelFormat.Format4bppIndexed;
        case ImagePixelFormat.BPP8Indexed: return System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
        case ImagePixelFormat.BPP16Gray:   return System.Drawing.Imaging.PixelFormat.Format16bppGrayScale;
        case ImagePixelFormat.RGB24:       return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
        case ImagePixelFormat.RGB32:       return System.Drawing.Imaging.PixelFormat.Format32bppRgb;
        case ImagePixelFormat.RGBA32:      return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
        default: return System.Drawing.Imaging.PixelFormat.Canonical;
      }
    }

    private static ImagePixelFormat xlat(System.Drawing.Imaging.PixelFormat pf)
    {
      switch(pf)
      {
        case  System.Drawing.Imaging.PixelFormat.Format1bppIndexed:     return ImagePixelFormat.BPP1Indexed;
        case  System.Drawing.Imaging.PixelFormat.Format4bppIndexed:     return ImagePixelFormat.BPP4Indexed;
        case  System.Drawing.Imaging.PixelFormat.Format8bppIndexed:     return ImagePixelFormat.BPP8Indexed;
        case  System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:  return ImagePixelFormat.BPP16Gray;
        case  System.Drawing.Imaging.PixelFormat.Format24bppRgb:        return ImagePixelFormat.RGB24;
        case  System.Drawing.Imaging.PixelFormat.Format32bppRgb:        return ImagePixelFormat.RGB32;
        case  System.Drawing.Imaging.PixelFormat.Format32bppArgb:       return ImagePixelFormat.RGBA32;
        default: return ImagePixelFormat.Default;
      }
    }
  }
}
