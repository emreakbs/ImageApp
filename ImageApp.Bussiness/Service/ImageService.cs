﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ImageApp.Bussiness.Service
{
    public class ImageService
    {
        #region Single Section

        private static readonly Lazy<ImageService> instance = new Lazy<ImageService>(() => new ImageService());
        public ImageService() { }
        public static ImageService Instance => instance.Value;

        #endregion


    }
}
