using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace RadioApp.Behaviors
{
    public class MouseChasingBehavior : Behavior<Grid>
    {
        public Popup MouseTarget
        {
            get { return (Popup)GetValue(MouseTargetProperty); }
            set { SetValue(MouseTargetProperty, value); }
        }

        public static readonly DependencyProperty MouseTargetProperty
            = DependencyProperty.RegisterAttached("MouseTarget", typeof(Popup), typeof(MouseChasingBehavior));


        public int TargetOffset
        {
            get { return (int)GetValue(TargetOffsetProperty); }
            set { SetValue(TargetOffsetProperty, value); }
        }

        public static readonly DependencyProperty TargetOffsetProperty
            = DependencyProperty.RegisterAttached("TargetOffset", typeof(int), typeof(MouseChasingBehavior));

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (MouseTarget != null)
            {
                var mousePosition = e.GetPosition(sender as FrameworkElement);
                MouseTarget.HorizontalOffset = mousePosition.X + TargetOffset;
                MouseTarget.VerticalOffset = mousePosition.Y + TargetOffset;
            }
        }
    }
}
