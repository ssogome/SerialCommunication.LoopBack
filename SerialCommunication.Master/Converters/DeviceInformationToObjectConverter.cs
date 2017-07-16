using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml.Data;

namespace SerialCommunication.Master.Converters
{
    public class DeviceInformationToObjectConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value as object;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var id = string.Empty;
            var deviceInformation = value as DeviceInformation;

            if (deviceInformation != null)
            {
                id = deviceInformation.Id;
            }

            return id;
        }
    }    

}
