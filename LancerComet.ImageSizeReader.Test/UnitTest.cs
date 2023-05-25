using System.IO;
using LancerComet.ImageSizeReader.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LancerComet.ImageSizeReader.Test {
  [TestClass]
  public class UnitTest {
    [TestMethod]
    public void Bmp () {
      var bmp = ImageSizeReaderUtil.GetDimensions(File.OpenRead("test.bmp"));
      Assert.AreEqual(499, bmp.Width);
      Assert.AreEqual(501, bmp.Height);
    }

    [TestMethod]
    public void Gif () {
      var gif = ImageSizeReaderUtil.GetDimensions(File.OpenRead("test.gif"));
      Assert.AreEqual(512, gif.Width);
      Assert.AreEqual(512, gif.Height);
    }

    [TestMethod]
    public void Jpg () {
      var jpg = ImageSizeReaderUtil.GetDimensions(File.OpenRead("test.jpg"));
      Assert.AreEqual(512, jpg.Width);
      Assert.AreEqual(512, jpg.Height);
    }

    [TestMethod]
    public void Png () {
      var png = ImageSizeReaderUtil.GetDimensions(File.OpenRead("test.png"));
      Assert.AreEqual(512, png.Width);
      Assert.AreEqual(512, png.Height);
    }

    [TestMethod]
    public void WebpLossless () {
      var webp = ImageSizeReaderUtil.GetDimensions(File.OpenRead("test.vp8l.webp"));
      Assert.AreEqual(512, webp.Width);
      Assert.AreEqual(512, webp.Height);
    }

    [TestMethod]
    public void WebpLossy () {
      var webp = ImageSizeReaderUtil.GetDimensions(File.OpenRead("test.vp8.webp"));
      Assert.AreEqual(512, webp.Width);
      Assert.AreEqual(512, webp.Height);
    }

    [TestMethod]
    public void WebpVP8X () {
      var webp = ImageSizeReaderUtil.GetDimensions(File.OpenRead("test.vp8x.webp"));
      Assert.AreEqual(1799, webp.Width);
      Assert.AreEqual(885, webp.Height);
    }
  }
}
