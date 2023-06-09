﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RevitProject
{
    public class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly Action _excute;
        private readonly Predicate<object> _canExcute;

        public Command(Action excute, Predicate<Object> canExcute = null)

        {
            _excute = excute;

            _canExcute = canExcute;
        }


        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _excute();
        }
    }
}
