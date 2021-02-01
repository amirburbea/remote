﻿using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Remote.Neeo.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public sealed class SecureController : ControllerBase
    {
        private readonly string _publicKey;

        public SecureController(PgpKeys keys)
        {
            this._publicKey = Encoding.ASCII.GetString(PgpMethods.GetKeyBytes(keys.PublicKey.Encode));
        }

        [HttpGet("pubkey")]
        public ActionResult<object> GetPublicKey() => this.Ok(new { Publickey = this._publicKey });
    }
}
