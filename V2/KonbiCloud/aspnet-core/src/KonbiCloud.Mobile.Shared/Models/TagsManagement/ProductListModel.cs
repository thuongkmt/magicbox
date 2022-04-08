using Abp.AutoMapper;
using KonbiCloud.Products.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace KonbiCloud.Models.TagsManagement
{
    [AutoMapFrom(typeof(ProductDto))]
    public class ProductListModel : ProductDto, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
