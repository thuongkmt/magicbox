using Abp.AutoMapper;
using KonbiCloud.Products.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace KonbiCloud.Models.TagsManagement
{
    [AutoMapFrom(typeof(ProductCategoryDto))]
    public class ProductCategoryListModel : ProductCategoryDto, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
