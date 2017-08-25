﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Resource.Icons
{
    public enum PackIconFlipOrientation
    {
        /// <summary>
        /// No flip
        /// </summary>
        Normal,

        /// <summary>
        /// Flip the icon horizontal
        /// </summary>
        Horizontal,

        /// <summary>
        /// Flip the icon vertical
        /// </summary>
        Vertical,

        /// <summary>
        /// Flip the icon vertical and horizontal
        /// </summary>
        Both
    }

    public class PackIcon : Control
    {
        static PackIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PackIcon), new FrameworkPropertyMetadata(typeof(PackIcon)));
            OpacityProperty.OverrideMetadata(typeof(PackIcon), new UIPropertyMetadata(1d, (d, e) => { d.CoerceValue(SpinProperty); }));
            VisibilityProperty.OverrideMetadata(typeof(PackIcon), new UIPropertyMetadata(Visibility.Visible, (d, e) => { d.CoerceValue(SpinProperty); }));
            
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateData(Kind);
            this.CoerceValue(SpinProperty);
            if (this.Spin)
            {
                this.StopSpinAnimation();
                this.BeginSpinAnimation();
            }
        }


        /// <summary>
        /// Identifies the Flip dependency property.
        /// </summary>
        public static readonly DependencyProperty FlipProperty
            = DependencyProperty.Register(
                "Flip",
                typeof(PackIconFlipOrientation),
                typeof(PackIcon),
                new PropertyMetadata(PackIconFlipOrientation.Normal));

        /// <summary>
        /// Gets or sets the flip orientation.
        /// </summary>
        public PackIconFlipOrientation Flip
        {
            get { return (PackIconFlipOrientation)GetValue(FlipProperty); }
            set { this.SetValue(FlipProperty, value); }
        }

        /// <summary>
        /// Identifies the Rotation dependency property.
        /// </summary>
        public static readonly DependencyProperty RotationProperty
            = DependencyProperty.Register(
                "Rotation",
                typeof(double),
                typeof(PackIcon),
                new PropertyMetadata(0d, null, RotationPropertyCoerceValueCallback));

        private static object RotationPropertyCoerceValueCallback(DependencyObject dependencyObject, object value)
        {
            var val = (double)value;
            return val < 0 ? 0d : (val > 360 ? 360d : value);
        }

        /// <summary>
        /// Gets or sets the rotation (angle).
        /// </summary>
        /// <value>The rotation.</value>
        public double Rotation
        {
            get { return (double)this.GetValue(RotationProperty); }
            set { this.SetValue(RotationProperty, value); }
        }

        /// <summary>
        /// Identifies the Spin dependency property.
        /// </summary>
        public static readonly DependencyProperty SpinProperty
            = DependencyProperty.Register(
                "Spin",
                typeof(bool),
                typeof(PackIcon),
                new PropertyMetadata(default(bool), SpinPropertyChangedCallback, SpinPropertyCoerceValueCallback));

        private static object SpinPropertyCoerceValueCallback(DependencyObject dependencyObject, object value)
        {
            var packIcon = dependencyObject as PackIcon;
            if (packIcon != null && (!packIcon.IsVisible || packIcon.Opacity <= 0 || packIcon.SpinDuration <= 0.0))
            {
                return false;
            }
            return value;
        }

        private static void SpinPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var packIcon = dependencyObject as PackIcon;
            if (packIcon != null && e.OldValue != e.NewValue && e.NewValue is bool)
            {
                var spin = (bool)e.NewValue;
                if (spin)
                {
                    packIcon.BeginSpinAnimation();
                }
                else
                {
                    packIcon.StopSpinAnimation();
                }
            }
        }

        private static readonly string SpinnerStoryBoardName = $"{typeof(PackIcon).Name}SpinnerStoryBoard";

        private FrameworkElement _innerGrid;
        private FrameworkElement InnerGrid => this._innerGrid ?? (this._innerGrid = this.GetTemplateChild("PART_InnerGrid") as FrameworkElement);

        private void BeginSpinAnimation()
        {
            var element = this.InnerGrid;
            if (null == element)
            {
                return;
            }
            var transformGroup = element.RenderTransform as TransformGroup ?? new TransformGroup();
            var rotateTransform = transformGroup.Children.OfType<RotateTransform>().LastOrDefault();

            if (rotateTransform != null)
            {
                rotateTransform.Angle = 0;
            }
            else
            {
                transformGroup.Children.Add(new RotateTransform());
                element.RenderTransform = transformGroup;
            }

            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                AutoReverse = this.SpinAutoReverse,
                EasingFunction = this.SpinEasingFunction,
                RepeatBehavior = RepeatBehavior.Forever,
                Duration = new Duration(TimeSpan.FromSeconds(this.SpinDuration))
            };
            storyboard.Children.Add(animation);

            Storyboard.SetTarget(animation, element);

            Storyboard.SetTargetProperty(animation, new PropertyPath("(0).(1)[2].(2)", RenderTransformProperty, TransformGroup.ChildrenProperty, RotateTransform.AngleProperty));

            element.Resources.Add(SpinnerStoryBoardName, storyboard);
            storyboard.Begin();
        }

        private void StopSpinAnimation()
        {
            var element = this.InnerGrid;
            if (null == element)
            {
                return;
            }
            var storyboard = element.Resources[SpinnerStoryBoardName] as Storyboard;
            if (storyboard != null)
            {
                storyboard.Stop();
                element.Resources.Remove(SpinnerStoryBoardName);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the inner icon is spinning.
        /// </summary>
        /// <value><c>true</c> if spin; otherwise, <c>false</c>.</value>
        public bool Spin
        {
            get { return (bool)this.GetValue(SpinProperty); }
            set { this.SetValue(SpinProperty, value); }
        }

        /// <summary>
        /// Identifies the SpinDuration dependency property.
        /// </summary>
        public static readonly DependencyProperty SpinDurationProperty
            = DependencyProperty.Register("SpinDuration",
                typeof(double),
                typeof(PackIcon),
                new PropertyMetadata(1d, SpinDurationPropertyChangedCallback, SpinDurationCoerceValueCallback));


        private static void SpinDurationPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var packIcon = dependencyObject as PackIcon;
            if (packIcon != null && e.OldValue != e.NewValue && packIcon.Spin && e.NewValue is double)
            {
                packIcon.StopSpinAnimation();
                packIcon.BeginSpinAnimation();
            }
        }
        
        private static object SpinDurationCoerceValueCallback(DependencyObject dependencyObject, object value)
        {
            var val = (double)value;
            return val < 0 ? 0d : value;
        }

        /// <summary>
        /// Gets or sets the duration of the spinning animation (in seconds). This will also restart the spin animation.
        /// </summary>
        /// <value>The duration of the spin in seconds.</value>
        public double SpinDuration
        {
            get { return (double)this.GetValue(SpinDurationProperty); }
            set { this.SetValue(SpinDurationProperty, value); }
        }

        /// <summary>
        /// Identifies the SpinEasingFunction dependency property.
        /// </summary>
        public static readonly DependencyProperty SpinEasingFunctionProperty
            = DependencyProperty.Register("SpinEasingFunction",
                typeof(IEasingFunction),
                typeof(PackIcon),
                new PropertyMetadata(null, SpinEasingFunctionPropertyChangedCallback));

        private static void SpinEasingFunctionPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var packIcon = dependencyObject as PackIcon;
            if (packIcon != null && e.OldValue != e.NewValue && packIcon.Spin)
            {
                packIcon.StopSpinAnimation();
                packIcon.BeginSpinAnimation();
            }
        }

        /// <summary>
        /// Gets or sets the EasingFunction of the spinning animation. This will also restart the spin animation.
        /// </summary>
        /// <value>The spin easing function.</value>

        public IEasingFunction SpinEasingFunction
        {
            get { return (IEasingFunction)this.GetValue(SpinEasingFunctionProperty); }
            set { this.SetValue(SpinEasingFunctionProperty, value); }
        }


        /// <summary>
        /// Identifies the SpinAutoReverse dependency property.
        /// </summary>
        public static readonly DependencyProperty SpinAutoReverseProperty
            = DependencyProperty.Register("SpinAutoReverse",
                typeof(bool),
                typeof(PackIcon),
                new PropertyMetadata(default(bool), SpinAutoReversePropertyChangedCallback));

        private static void SpinAutoReversePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var packIcon = dependencyObject as PackIcon;
            if (packIcon != null && e.OldValue != e.NewValue && packIcon.Spin && e.NewValue is bool)
            {
                packIcon.StopSpinAnimation();
                packIcon.BeginSpinAnimation();
            }
        }

        /// <summary>
        /// Gets or sets the AutoReverse of the spinning animation. This will also restart the spin animation.
        /// </summary>
        /// <value><c>true</c> if [spin automatic reverse]; otherwise, <c>false</c>.</value>
        public bool SpinAutoReverse
        {
            get { return (bool)this.GetValue(SpinAutoReverseProperty); }
            set { this.SetValue(SpinAutoReverseProperty, value); }
        }

        public static readonly DependencyProperty KindProperty
            = DependencyProperty.Register(nameof(Kind), typeof(PackIconKind), typeof(PackIcon), new PropertyMetadata(default(PackIconKind), KindPropertyChangedCallback));

        private static void KindPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyPropertyChangedEventArgs.NewValue is PackIconKind)
            {
                ((PackIcon)dependencyObject).UpdateData((PackIconKind)dependencyPropertyChangedEventArgs.NewValue);
            }
        }

        /// <summary>
        /// Gets or sets the icon to display.
        /// </summary>
        public PackIconKind Kind
        {
            get { return (PackIconKind)GetValue(KindProperty); }
            set { SetValue(KindProperty, value); }
        }


        public static readonly DependencyProperty StateProperty
           = DependencyProperty.Register(nameof(State), typeof(PackIconState), typeof(PackIcon), new PropertyMetadata(default(PackIconState), StatePropertyChangedCallback));

        private static void StatePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyPropertyChangedEventArgs.NewValue is PackIconState)
            {
                ((PackIcon)dependencyObject).SetCurrentDict((PackIconState)dependencyPropertyChangedEventArgs.NewValue);
            }
        }

        void SetCurrentDict(PackIconState state)
        {
            CurrentDict = PackIconDataFactory.DataFactoryDict[state];
            OnApplyTemplate();
        }

        private IDictionary<PackIconKind, string> CurrentDict = new Dictionary<PackIconKind,string>();

        /// <summary>
        /// Gets or sets the icon to display.
        /// </summary>
        public PackIconState State
        {
            get { return (PackIconState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }
        private static readonly DependencyPropertyKey DataPropertyKey
            = DependencyProperty.RegisterReadOnly(nameof(Data), typeof(string), typeof(PackIcon), new PropertyMetadata(""));

        // ReSharper disable once StaticMemberInGenericType
        public static readonly DependencyProperty DataProperty = DataPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the icon path data for the current <see cref="Kind"/>.
        /// </summary>
        [TypeConverter(typeof(GeometryConverter))]
        public string Data
        {
            get { return (string)GetValue(DataProperty); }
            private set { SetValue(DataPropertyKey, value); }
        }

        internal void UpdateData(PackIconKind kind)
        {
            string data = null;
            if(CurrentDict.TryGetValue(kind,out data))
                Data = data;
            Data = data;
        }
    }
}
