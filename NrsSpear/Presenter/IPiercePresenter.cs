using System;
using NrsSpear.Client;
using NrsSpear.Client.Setting;

namespace NrsSpear.Presenter
{
    public interface IPiercePresenter
    {
        void Handle(DateTime time, string target, SpearTask task, PierceSetting setting, int count);
    }
}
