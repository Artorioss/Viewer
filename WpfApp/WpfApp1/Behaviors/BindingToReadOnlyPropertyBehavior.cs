using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Data;

namespace WpfApp1.Behaviors
{
    public class BindingToReadOnlyPropertyBehavior : Behavior<DependencyObject>
    {
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(object), typeof(BindingToReadOnlyPropertyBehavior), new PropertyMetadata(null));
        private static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(BindingToReadOnlyPropertyBehavior), new PropertyMetadata(null, propertyChangedCallback));
        private readonly Binding binding = new Binding("") { Mode = BindingMode.OneWay };
        private static void propertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BindingToReadOnlyPropertyBehavior)d).Target = e.NewValue;
        }
       
        public object Target
        {
            get { return (object)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        public PropertyPath Property 
        { 
            get => binding.Path; 
            set => binding.Path = value; 
        }

        protected override void OnAttached()
        {
            binding.Source = AssociatedObject;
            BindingOperations.SetBinding(this, SourceProperty, binding);
        }
    }
}
