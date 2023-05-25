namespace LancerComet.ImageSizeReader.Model; 

public class Size {
  public int Width { get; }

  public int Height { get; }

  public Size (int width, int height) {
    this.Width = width;
    this.Height = height;
  }
}