using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Caliburn.Micro;
using KonbiBrain.Messages;
using RawInputBrain;
using RawInputBrain.ViewModels;
using Newtonsoft.Json;

namespace RawInputBrain
{
    public class RawInputController : ApiController
    {

        public string Get()
        {
            return "pong";
        }
    }
}
