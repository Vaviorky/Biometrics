using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Biometrics.Classes
{
    public static class Minutia
    {
        private static int _width, _height, _stride;
        private static byte[] _pixels;
        private static int[,] _intPixels, _tempIntPixels;

        public static BitmapSource MarkMinuties(BitmapSource source)
        {
            var bitmap = new WriteableBitmap(source);

            _width = bitmap.PixelWidth;
            _height = bitmap.PixelHeight;

            _stride = _width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = _stride * _height;
            _pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(_pixels, _stride, 0);

            InitIntImage();
            _tempIntPixels = new int[_width, _height];
            for (var i = 0; i < _width; i++)
                for (var j = 0; j < _height; j++)
                    _tempIntPixels[i, j] = _intPixels[i, j];

            for (var x = 0; x < _width; x++)
                for (var y = 0; y < _height; y++)
                {
                    int countForSquare5 = 0, countForSquare9 = 0;
                    bool square5 = false, square9 = false;

                    if (_intPixels[x, y] != 1)
                        continue;

                    var blacksInSquare5 = MinutiaeHelpers.GetArrayOfBlacksSquare5(x, y, _intPixels);
                    var blacksInSquare9 = MinutiaeHelpers.GetArrayOfBlacksSquare9(x, y, _intPixels);

                    foreach (var pixel in blacksInSquare5)
                    {
                        if (pixel == 1)
                            square5 = true;
                        if (pixel == 0 && square5)
                        {
                            countForSquare5++;
                            square5 = false;
                        }
                    }

                    foreach (var pixel in blacksInSquare9)
                    {
                        if (pixel == 1)
                            square9 = true;
                        if (pixel == 0 && square9)
                        {
                            countForSquare9++;
                            square9 = false;
                        }
                    }

                    //MarkEndings(x, y);

                    //if both squares are crossovers exactly 3 times
                    if (countForSquare5 == 3 && countForSquare9 == 3)
                        _tempIntPixels[x, y] = 2;
                }

            RevertIntPixelsIntoPixelArray();
            RevertTempIntPixelsIntoArray();

            var rect = new Int32Rect(0, 0, _width, _height);
            bitmap.WritePixels(rect, _pixels, _stride, 0);
            return bitmap;
        }

        public static BitmapSource DeleteRepetatiobns(BitmapSource source)
        {
            var bitmap = new WriteableBitmap(source);

            _width = bitmap.PixelWidth;
            _height = bitmap.PixelHeight;

            _stride = _width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = _stride * _height;
            _pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(_pixels, _stride, 0);

            InitIntImage();

            _tempIntPixels = new int[_width, _height];
            for (var i = 0; i < _width; i++)
                for (var j = 0; j < _height; j++)
                    _tempIntPixels[i, j] = _intPixels[i, j];

            for (var x = 0; x < _width; x++)
                for (var y = 0; y < _height; y++)
                    if (_intPixels[x, y] == 2)
                    {
                        var minutiaesInSingleArea = new List<int[]>();
                        for (var i = -2; i <= 2; i++)
                            for (var j = -2; j <= 2; j++)
                            {
                                // if it goes out of image
                                if (x + i < 0 || x + i >= _width || y + j < 0 ||
                                    y + j >= _height /*|| (i == 0 && j == 0)*/)
                                    continue;

                                //get area of red pixels (candidates for minutiae)
                                if (_intPixels[x + i, y + j] == 2)
                                {
                                    var posX = x + i;
                                    var posY = y + j;

                                    var pixel = new int[2];
                                    pixel[0] = posX;
                                    pixel[1] = posY;

                                    minutiaesInSingleArea.Add(pixel);
                                }
                            }

                        //remove unecessary candidates
                        foreach (var pixel in minutiaesInSingleArea)
                        {
                            var tempX = pixel[0];
                            var tempY = pixel[1];

                            var redCount = 0;
                            if (_intPixels[tempX - 1, tempY] == 2)
                                redCount++;

                            if (_intPixels[tempX + 1, tempY] == 2)
                                redCount++;

                            if (_intPixels[tempX, tempY - 1] == 2)
                                redCount++;

                            if (_intPixels[tempX, tempY + 1] == 2)
                                redCount++;

                            if (redCount < 2)
                                _tempIntPixels[tempX, tempY] = 1;

                            if (redCount == 2)
                                _tempIntPixels[tempX, tempY] = 2;
                        }
                    }

            RevertIntPixelsIntoPixelArray();
            RevertTempIntPixelsIntoArray();

            var rect = new Int32Rect(0, 0, _width, _height);
            bitmap.WritePixels(rect, _pixels, _stride, 0);
            return bitmap;
        }

        public static BitmapSource RemoveRedundantMinutiaes(BitmapSource source)
        {
            var bitmap = new WriteableBitmap(source);

            _width = bitmap.PixelWidth;
            _height = bitmap.PixelHeight;

            _stride = _width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = _stride * _height;
            _pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(_pixels, _stride, 0);

            InitIntImage();

            var candidates = new int[_width, _height];
            //remove redundant red pixels
            for (var x = 0; x < _width; x++)
                for (var y = 0; y < _height; y++)
                    if (_intPixels[x, y] == 2)
                    {
                        var redCount = 0;
                        if (_intPixels[x - 1, y] == 1 || _intPixels[x - 1, y] == 2)
                            redCount++;

                        if (_intPixels[x + 1, y] == 1 || _intPixels[x + 1, y] == 2)
                            redCount++;

                        if (_intPixels[x, y - 1] == 1 || _intPixels[x, y - 1] == 2)
                            redCount++;

                        if (_intPixels[x, y + 1] == 1 || _intPixels[x, y + 1] == 2)
                            redCount++;

                        if (redCount == 2)
                            candidates[x, y] = 2;
                        else if (redCount == 3)
                            candidates[x, y] = 3;
                        else if (redCount == 4)
                            candidates[x, y] = 3;
                    }

            for (var x = 0; x < _width; x++)
                for (var y = 0; y < _height; y++)
                    if (candidates[x, y] > 0)
                    {
                        if (candidates[x - 1, y] > 0 && candidates[x + 1, y] > 0)
                        {
                            _intPixels[x - 1, y] = 1;
                            _intPixels[x + 1, y] = 1;
                        }

                        if (candidates[x, y - 1] > 0 && candidates[x, y + 1] > 0)
                        {
                            _intPixels[x, y - 1] = 1;
                            _intPixels[x, y + 1] = 1;
                        }
                    }

            RevertIntPixelsIntoPixelArray();

            //delete one of two red neighbours
            InitIntImage();
            for (var x = 0; x < _width; x++)
                for (var y = 0; y < _height; y++)
                    if (_intPixels[x, y] == 2)
                    {
                        var maxNeigbours = new int[2];
                        maxNeigbours[0] = x;
                        maxNeigbours[1] = y;
                        var count = CountNeighbours(x, y);

                        if (_intPixels[x - 1, y] == 2 && CountNeighbours(x - 1, y) > count)
                        {
                            maxNeigbours[0] = x - 1;
                            maxNeigbours[1] = y;
                        }
                        if (_intPixels[x + 1, y] == 2 && CountNeighbours(x + 1, y) > count)
                        {
                            maxNeigbours[0] = x + 1;
                            maxNeigbours[1] = y;
                        }

                        if (_intPixels[x, y - 1] == 2 && CountNeighbours(x, y - 1) > count)
                        {
                            maxNeigbours[0] = x;
                            maxNeigbours[1] = y - 1;
                        }
                        if (_intPixels[x, y + 1] == 2 && CountNeighbours(x, y + 1) > count)
                        {
                            maxNeigbours[0] = x;
                            maxNeigbours[1] = y + 1;
                        }

                        for (var i = -1; i <= 1; i++)
                            for (var j = -1; j <= 1; j++)
                                if (_intPixels[x + i, y + j] == 2)
                                    _intPixels[x + i, y + j] = 1;
                        _intPixels[maxNeigbours[0], maxNeigbours[1]] = 2;
                    }

            RevertIntPixelsIntoPixelArray();

            var rect = new Int32Rect(0, 0, _width, _height);
            bitmap.WritePixels(rect, _pixels, _stride, 0);
            return bitmap;
        }

        public static BitmapSource DeleteFalseMinutiaes(BitmapSource source, int minLength)
        {
            var bitmap = new WriteableBitmap(source);

            _width = bitmap.PixelWidth;
            _height = bitmap.PixelHeight;

            _stride = _width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = _stride * _height;
            _pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(_pixels, _stride, 0);

            InitIntImage();

            for (var x = 0; x < _width; x++)
                for (var y = 0; y < _height; y++)
                    if (_intPixels[x, y] == 2)
                    {
                        int counter = 0;
                        if (_intPixels[x - 1, y] == 0)
                        {
                            counter++;
                        }
                        else
                        {
                            bool check = IsPathLongEnoughtForMinutiae(x, y, x - 1, y, minLength);
                            if (check)
                            {
                                counter++;
                            }
                        }

                        if (_intPixels[x + 1, y] == 0)
                        {
                            counter++;
                        }
                        else
                        {
                            bool check = IsPathLongEnoughtForMinutiae(x, y, x + 1, y, minLength);
                            if (check)
                            {
                                counter++;
                            }
                        }

                        if (_intPixels[x, y - 1] == 0)
                        {
                            counter++;
                        }
                        else
                        {
                            bool check = IsPathLongEnoughtForMinutiae(x, y, x, y - 1, minLength);
                            if (check)
                            {
                                counter++;
                            }
                        }

                        if (_intPixels[x, y + 1] == 0)
                        {
                            counter++;
                        }
                        else
                        {
                            bool check = IsPathLongEnoughtForMinutiae(x, y, x, y + 1, minLength);
                            if (check)
                            {
                                counter++;
                            }
                        }

                        if (counter < 4)
                        {
                            _intPixels[x, y] = 1;
                        }
                    }

            RevertIntPixelsIntoPixelArray();

            var rect = new Int32Rect(0, 0, _width, _height);
            bitmap.WritePixels(rect, _pixels, _stride, 0);
            return bitmap;
        }

        public static BitmapSource MarkEndings(BitmapSource source)
        {
            var bitmap = new WriteableBitmap(source);

            _width = bitmap.PixelWidth;
            _height = bitmap.PixelHeight;

            _stride = _width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = _stride * _height;
            _pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(_pixels, _stride, 0);

            InitIntImage();

            _tempIntPixels = new int[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _tempIntPixels[x, y] = _intPixels[x, y];
                }
            }


            for (int x = 50; x < _width - 50; x++)
            {
                for (int y = 50; y < _height - 50; y++)
                {
                    if (_intPixels[x, y] == 1 && CountNeighbours(x, y) == 1)
                    {
                        _tempIntPixels[x, y] = 3;
                    }
                }
            }

            RevertTempIntPixelsIntoArray();

            var rect = new Int32Rect(0, 0, _width, _height);
            bitmap.WritePixels(rect, _pixels, _stride, 0);
            return bitmap;
        }

        private static void InitIntImage()
        {
            _intPixels = new int[_width, _height];

            for (var x = 0; x < _width; x++)
                for (var y = 0; y < _height; y++)
                {
                    var j = PositionInArray(x, y);

                    if (_pixels[j + 2] == 255 && _pixels[j + 1] == 255 && _pixels[j] == 255)
                        _intPixels[x, y] = 0;
                    else if (_pixels[j + 2] == 0 && _pixels[j + 1] == 0 && _pixels[j] == 0)
                        _intPixels[x, y] = 1;
                    else if (_pixels[j + 2] == 255 && _pixels[j + 1] == 0 && _pixels[j] == 0)
                        _intPixels[x, y] = 2;
                }
        }

        private static bool IsPathLongEnoughtForMinutiae(int redX, int redY, int x, int y, int minCount)
        {
            var currX = x;
            var currY = y;

            var visitedPixelsList = new List<string>();
            var isLongEnough = false;

            //save red pixel
            visitedPixelsList.Add(redX + "x" + redY);

            if (_intPixels[x, y] == 1)
            {
                visitedPixelsList.Add(currX + "x" + currY);

                var onlyWhiteLast = true;

                do
                {
                    if (_intPixels[currX - 1, currY] == 1 && CountNeighbours(currX - 1, currY) == 2 && !visitedPixelsList.Contains(currX - 1 + "x" + currY))
                    {
                        currX--;
                        visitedPixelsList.Add(currX + "x" + currY);
                        continue;
                    }
                    if (_intPixels[currX + 1, currY] == 1 && CountNeighbours(currX + 1, currY) == 2 && !visitedPixelsList.Contains(currX + 1 + "x" + currY))
                    {
                        currX++;
                        visitedPixelsList.Add(currX + "x" + currY);
                        continue;
                    }
                    if (_intPixels[currX, currY - 1] == 1 && CountNeighbours(currX, currY - 1) == 2 && !visitedPixelsList.Contains(currX + "x" + (currY - 1)))
                    {
                        currY--;
                        visitedPixelsList.Add(currX + "x" + currY);
                        continue;
                    }

                    if (_intPixels[currX, currY + 1] == 1 && CountNeighbours(currX, currY + 1) == 2 && !visitedPixelsList.Contains(currX + "x" + (currY + 1)))
                    {
                        currY++;
                        visitedPixelsList.Add(currX + "x" + currY);
                        continue;
                    }

                    if (CountNeighbours(currX - 1, currY) == 1)
                        onlyWhiteLast = false;

                    if (CountNeighbours(currX + 1, currY) == 1)
                        onlyWhiteLast = false;

                    if (CountNeighbours(currX, currY - 1) == 1)
                        onlyWhiteLast = false;

                    if (CountNeighbours(currX, currY + 1) == 1)
                        onlyWhiteLast = false;


                    if (CountNeighbours(currX - 1, currY) == 3)
                    {
                        isLongEnough = true;
                        break;
                    }

                    if (CountNeighbours(currX + 1, currY) == 3)
                    {
                        isLongEnough = true;
                        break;
                    }

                    if (CountNeighbours(currX, currY - 1) == 3)
                    {
                        isLongEnough = true;
                        break;
                    }

                    if (CountNeighbours(currX, currY + 1) == 3)
                    {
                        isLongEnough = true;
                        break;
                    }

                    if (visitedPixelsList.Count >= minCount)
                    {
                        isLongEnough = true;
                        break;
                    }

                } while (onlyWhiteLast);
            }

            return isLongEnough;
        }

        private static int CountNeighbours(int x, int y)
        {
            var count = 0;
            if (_intPixels[x - 1, y] == 1 || _intPixels[x - 1, y] == 2)
                count++;
            if (_intPixels[x + 1, y] == 1 || _intPixels[x + 1, y] == 2)
                count++;
            if (_intPixels[x, y - 1] == 1 || _intPixels[x, y - 1] == 2)
                count++;
            if (_intPixels[x, y + 1] == 1 || _intPixels[x, y + 1] == 2)
                count++;

            return count;
        }

        private static void RevertIntPixelsIntoPixelArray()
        {
            for (var x = 0; x < _width; x++)
                for (var y = 0; y < _height; y++)
                {
                    var j = PositionInArray(x, y);

                    if (_intPixels[x, y] == 0)
                    {
                        _pixels[j + 2] = 255;
                        _pixels[j + 1] = 255;
                        _pixels[j] = 255;
                    }

                    if (_intPixels[x, y] == 1)
                    {
                        _pixels[j + 2] = 0;
                        _pixels[j + 1] = 0;
                        _pixels[j] = 0;
                    }

                    if (_intPixels[x, y] == 2)
                    {
                        _pixels[j + 2] = 255;
                        _pixels[j + 1] = 0;
                        _pixels[j] = 0;
                    }

                    if (_intPixels[x, y] == 3)
                    {
                        _pixels[j + 2] = 0;
                        _pixels[j + 1] = 255;
                        _pixels[j] = 0;
                    }
                }
        }

        private static void RevertTempIntPixelsIntoArray()
        {
            for (var x = 0; x < _width; x++)
                for (var y = 0; y < _height; y++)
                {
                    var j = PositionInArray(x, y);
                    if (_tempIntPixels[x, y] == 0)
                    {
                        _pixels[j + 2] = 255;
                        _pixels[j + 1] = 255;
                        _pixels[j] = 255;
                    }

                    if (_tempIntPixels[x, y] == 1)
                    {
                        _pixels[j + 2] = 0;
                        _pixels[j + 1] = 0;
                        _pixels[j] = 0;
                    }

                    if (_tempIntPixels[x, y] == 2)
                    {
                        _pixels[j + 2] = 255;
                        _pixels[j + 1] = 0;
                        _pixels[j] = 0;
                    }

                    if (_tempIntPixels[x, y] == 3)
                    {
                        _pixels[j + 2] = 0;
                        _pixels[j + 1] = 255;
                        _pixels[j] = 0;
                    }
                }
        }

        private static int PositionInArray(int x, int y)
        {
            return 4 * x + y * _stride;
        }
    }
}