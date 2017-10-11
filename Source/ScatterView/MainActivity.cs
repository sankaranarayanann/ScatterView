using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.OS;
using ScatterView.Helper;
using Android.Hardware;
using ScatterView.Shared.ViewModel;
using System.Threading.Tasks;
using ScatterView.Shared.Helper;
using Android.Util;

namespace ScatterView
{
    [Activity(Label = AppConstant.AppName, MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {

        #region Variables
        /// <summary>
        /// FrameLayout as main layout to hold all views
        /// </summary>
        private FrameLayout mainLayout;
        /// <summary>
        /// Limit the app from multiple event wiring
        /// </summary>
        private bool isParentEventsWired = false;

        /// <summary>
        /// Double click time delta
        /// </summary>
        private const long DOUBLE_CLICK_TIME_DELTA = 200;

        /// <summary>
        /// Holds the last clicked time till the second click occures
        /// </summary>
        private long lastClickTime = 0;

        /// <summary>
        /// Fields for detect device shake
        /// </summary>
        private SensorManager sensorService;
        private Sensor sensor;
        private ShakeDetector shakeDetector;

        /// <summary>
        /// Fields for loading random views
        /// </summary>
        private ViewGeneratorViewModel viewGeneratorVM;
        #endregion

        #region Override Methods
        /// <summary>
        /// Override called when the activity is starting
        /// </summary>
        /// <param name="bundle"></param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get main layout from the layout resource
            mainLayout = FindViewById<FrameLayout>(Resource.Id.mainLayout);

            // Initilize shake detecting objects
            sensorService = (SensorManager)GetSystemService(SensorService);
            sensor = sensorService.GetDefaultSensor(SensorType.Accelerometer);
            shakeDetector = new ShakeDetector();

            // Initilize the view's model
            viewGeneratorVM = new ViewGeneratorViewModel();
            // Calling the async with new thread will show warnings. To avoid it, using Task.Factory.StartNew
            Task.Factory.StartNew(async () => await viewGeneratorVM.InitializeAsync()); 
        }
        /// <summary>
        /// Override called when the app resumes
        /// </summary>
        protected override void OnResume()
        {
            WireParentEvents();
            sensorService = (SensorManager)GetSystemService(SensorService);
            sensorService.RegisterListener(shakeDetector, sensor, SensorDelay.Ui);
            base.OnResume();
        }
        /// <summary>
        /// Override called when the app goes on background
        /// </summary>
        protected override void OnPause()
        {
            UnWireParentEvents();
            sensorService = (SensorManager)GetSystemService(SensorService);
            sensorService.UnregisterListener(shakeDetector);
            base.OnPause();
        }
        #endregion

        #region Events
        /// <summary>
        /// Event invokes when touch on main layout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMainLayoutTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    if (viewGeneratorVM.ScatterViews.Count > 0)
                    {
                        // Getting the model values from view model
                        var scatterModel = viewGeneratorVM.ScatterViews[new Random().Next(viewGeneratorVM.ScatterViews.Count - 1)];
                        var view = new ImageView(this)
                        {
                            LayoutParameters = new ViewGroup.LayoutParams(scatterModel.Size.Width, scatterModel.Size.Height)
                        };
                        // To make x and y as center, difference of (x or y) and view's (width or height) and divide by 2
                        view.SetX(e.Event.GetX() - (view.LayoutParameters.Width / 2));
                        view.SetY(e.Event.GetY() - (view.LayoutParameters.Height / 2));
                        view.SetImageBitmap(scatterModel.Image);
                        // Reference will be removed while removing the views from parent layout
                        view.Touch += OnChildViewTouch;
                        view.LongClick += OnChildViewLongClick;
                        Title = scatterModel.Title;
                        // Framelayout will holds multiple child view
                        mainLayout.AddView(view);
                    }
                    else
                        Toast.MakeText(this, AppConstant.LoadingMessage, ToastLength.Short).Show();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Log.Error(AppConstant.AppName, ex.Message, ex);
#endif
            }
        }

        /// <summary>
        /// Invokes when the child view was pressed long
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChildViewLongClick(object sender, View.LongClickEventArgs e)
        {
            try
            {
                View view = (View)sender;
                // Will be unwired after drag completed
                view.Drag += OnChildViewDrag;
                View.DragShadowBuilder dragShadow = new View.DragShadowBuilder(view);
                // Gets the view and starting the drag and drop
                view.StartDragAndDrop(ClipData.NewPlainText("", ""), dragShadow, null, 0);
            }
            catch (Exception ex)
            {
#if DEBUG
                Log.Error(AppConstant.AppName, ex.Message, ex);
#endif
            }
        }

        /// <summary>
        /// Event invokes when touch on child view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChildViewTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event.Action == MotionEventActions.Up)
                {
                    // By default Android not recommending double click
                    // monitor time difference for handle double click
                    long clickTime = DateTime.Now.Millisecond;
                    if (clickTime - lastClickTime < DOUBLE_CLICK_TIME_DELTA)
                    {
                        if (viewGeneratorVM.ScatterViews.Count > 0)
                        {
                            // Updating the view on double click
                            var scatterModel = viewGeneratorVM.ScatterViews[new Random().Next(viewGeneratorVM.ScatterViews.Count - 1)];
                            var view = (ImageView)sender;
                            view.SetImageBitmap(scatterModel.Image);
                            Title = scatterModel.Title;
                        }
                    }
                    lastClickTime = clickTime;
                }
                e.Handled = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                Log.Error(AppConstant.AppName, ex.Message, ex);
#endif
            }
            
        }

        /// <summary>
        /// Event invokes when child view dragged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChildViewDrag(object sender, View.DragEventArgs e)
        {
            try
            {
                // To handle drag end activity either on DragAction Dropped or Ended
                if ((e.Event.Action == DragAction.Drop) || (e.Event.Action == DragAction.Ended))
                {
                    View view = (View)sender;
                    // Updating the views with cordinates
                    view.SetX(e.Event.GetX() - view.LayoutParameters.Width);
                    view.SetY(e.Event.GetY() - view.LayoutParameters.Height);
                    view.Drag -= OnChildViewDrag;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Log.Error(AppConstant.AppName, ex.Message, ex);
#endif
            }
        }

        /// <summary>
        /// Event invokes when the device shaked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="shakeCount"></param>
        private void OnDeviceShaked(object sender, int shakeCount)
        {
            // Removes all the child views and update the title with app name
            mainLayout.RemoveAllViews();
            Title = AppConstant.AppName;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Method to wire events
        /// </summary>
        private void WireParentEvents()
        {
            if (!isParentEventsWired)
            {
                mainLayout.Touch += OnMainLayoutTouch;
                shakeDetector.Shaked += OnDeviceShaked;
                isParentEventsWired = true;
            }
        }

        /// <summary>
        /// Method to un wire events
        /// </summary>
        private void UnWireParentEvents()
        {
            if (isParentEventsWired)
            {
                mainLayout.Touch -= OnMainLayoutTouch;
                shakeDetector.Shaked -= OnDeviceShaked;
                isParentEventsWired = false;
            }
        }

        #endregion
    }
}

