using Android.Graphics;
using System;
using System.Drawing;

namespace ScatterView.Shared.Model
{
    /// <summary>
    /// Model class for the scatter view
    /// </summary>
    public class ScatterModel
    {
        #region Fields
        // Back fields for properties
        private bool isSquareShape;
        private int randomSize;
        #endregion

        #region ctor
        public ScatterModel()
        {
            // Initializing with values for shape and size
            isSquareShape = (new Random().Next(10) % 2) > 0;
            randomSize = new Random().Next(100, 300); 
        }
        #endregion

        #region Properties
        /// <summary>
        /// Property to return  whether it is square shape
        /// </summary>
        public bool IsSquareShape
        {
            get { return isSquareShape; }
        }

        /// <summary>
        /// Title of the view
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Image for the view
        /// </summary>
        public Bitmap Image { get; set; }
        
        /// <summary>
        /// Size of the view
        /// </summary>
        public Size Size
        {
            get { return new Size(randomSize, randomSize); }
        }
        #endregion
    }
}
