using System.Runtime.CompilerServices;
using LANPaint.MVVM;
using Xunit;

namespace LANPaint.UnitTests
{
    public class BindableBaseTests
    {
        [Fact]
        public void NotifyPropertyChanged_CallWithMemberName()
        {
            var isRaised = false;
            var propertyNameEventArg = string.Empty;

            const string memberName = "SomeMember";
            var bindableBase = new BindableBaseStub();
            bindableBase.PropertyChanged += (sender, args) =>
            {
                isRaised = true;
                propertyNameEventArg = args.PropertyName;
            };

            bindableBase.NotifyPropertyChanged(memberName);

            Assert.True(isRaised);
            Assert.Equal(memberName, propertyNameEventArg);
        }

        //It's useless cause we test is stub actually has CallerMemberNameAttribute applied to the parameter.
        [Fact]
        public void NotifyPropertyChanged_CallWithDefault()
        {
            var isRaised = false;
            var propertyNameEventArg = string.Empty;

            var bindableBase = new BindableBaseStub();
            bindableBase.PropertyChanged += (sender, args) =>
            {
                isRaised = true;
                propertyNameEventArg = args.PropertyName;
            };

            bindableBase.NotifyPropertyChanged();

            Assert.True(isRaised);
            Assert.Equal(nameof(NotifyPropertyChanged_CallWithDefault), propertyNameEventArg);
        }

        [Fact]
        public void SetProperty_CallWithMemberName()
        {
            var isRaised = false;
            var propertyNameEventArg = string.Empty;
            var propertyToSet = string.Empty;
            const string valueToSet = "newValue";

            var bindableBase = new BindableBaseStub();
            bindableBase.PropertyChanged += (sender, args) =>
            {
                isRaised = true;
                propertyNameEventArg = args.PropertyName;
            };

            var setPropertyResult = bindableBase.SetProperty(ref propertyToSet, valueToSet, nameof(propertyToSet));

            Assert.True(isRaised);
            Assert.Equal(nameof(propertyToSet), propertyNameEventArg);
            Assert.Equal(valueToSet, propertyToSet);
            Assert.True(setPropertyResult);
        }

        //It's useless cause we test is stub actually has CallerMemberNameAttribute applied to the parameter.
        [Fact]
        public void SetProperty_CallWithDefault()
        {
            var isRaised = false;
            var propertyNameEventArg = string.Empty;
            var propertyToSet = string.Empty;
            const string valueToSet = "newValue";

            var bindableBase = new BindableBaseStub();
            bindableBase.PropertyChanged += (sender, args) =>
            {
                isRaised = true;
                propertyNameEventArg = args.PropertyName;
            };

            var setPropertyResult = bindableBase.SetProperty(ref propertyToSet, valueToSet);

            Assert.True(isRaised);
            Assert.Equal(nameof(SetProperty_CallWithDefault), propertyNameEventArg);
            Assert.Equal(valueToSet, propertyToSet);
            Assert.True(setPropertyResult);
        }

        [Fact]
        public void SetProperty_SetTheSameValue()
        {
            var isRaised = false;
            var propertyNameEventArg = string.Empty;
            var propertyToSet = "propertyValue";
            const string valueToSet = "propertyValue";
            
            var bindableBase = new BindableBaseStub();
            bindableBase.PropertyChanged += (sender, args) =>
            {
                isRaised = true;
                propertyNameEventArg = args.PropertyName;
            };

            var setPropertyResult = bindableBase.SetProperty(ref propertyToSet, valueToSet);

            Assert.False(isRaised);
            Assert.Equal(string.Empty, propertyNameEventArg);
            Assert.Equal(valueToSet, propertyToSet);
            Assert.False(setPropertyResult);
        }
        
        private class BindableBaseStub : BindableBase
        {
            public new bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
            {
                return base.SetProperty<T>(ref storage, value, propertyName);
            }

            public new void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            {
                base.NotifyPropertyChanged(propertyName);
            }
        }
    }
}