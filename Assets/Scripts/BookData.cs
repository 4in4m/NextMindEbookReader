using System;

[Serializable]
public class BookData
{
    public string name;
    public string path;
    public int lastReadingPoint;    // порядковый номер символа, на котором была открыта книга в последний раз 
}
