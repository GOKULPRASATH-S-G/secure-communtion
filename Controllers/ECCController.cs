// Controllers/ECCController.cs (Corrected and Complete)
using Microsoft.AspNetCore.Mvc;
using SecureCommECC.Services;

namespace SecureCommECC.Controllers
{
    public class KeyPairResponse
    {
        public string PublicKey { get; set; } = "";
        public string PrivateKey { get; set; } = "";
    }

    public class ECCRequest
    {
        public string Message { get; set; } = "";
        public string MyPrivateKey { get; set; } = "";
        public string OtherPublicKey { get; set; } = "";
    }
    
    public class HistoryRequest
    {
        public string HistoryJson { get; set; } = "";
        public string MyPrivateKey { get; set; } = "";
        public string OtherPublicKey { get; set; } = "";
    }

    [ApiController]
    [Route("api/ecc")]
    public class ECCController : ControllerBase
    {
        private readonly ECCService _ecc;
        private readonly AIService _ai;

        public ECCController(ECCService ecc, AIService ai)
        {
            _ecc = ecc;
            _ai = ai;
        }

        [HttpPost("generatekeys")]
        public IActionResult GenerateKeys()
        {
            // This method now returns a value on all paths.
            var (privateKey, publicKey) = _ecc.GenerateNewKeyPair();
            var response = new KeyPairResponse
            {
                PrivateKey = Convert.ToBase64String(privateKey),
                PublicKey = Convert.ToBase64String(publicKey)
            };
            return Ok(response);
        }
        
        [HttpPost("encrypt")]
        public IActionResult Encrypt([FromBody] ECCRequest req)
        {
            // This method now returns a value on all paths.
            try
            {
                var myPrivateKeyBytes = Convert.FromBase64String(req.MyPrivateKey);
                var otherPublicKeyBytes = Convert.FromBase64String(req.OtherPublicKey);
                var encrypted = _ecc.Encrypt(req.Message, myPrivateKeyBytes, otherPublicKeyBytes);
                return Ok(Convert.ToBase64String(encrypted));
            }
            catch 
            { 
                return BadRequest("Encryption failed. Check keys."); 
            }
        }

        [HttpPost("decrypt")]
        public IActionResult Decrypt([FromBody] ECCRequest req)
        {
            // This method now returns a value on all paths.
            try
            {
                var myPrivateKeyBytes = Convert.FromBase64String(req.MyPrivateKey);
                var otherPublicKeyBytes = Convert.FromBase64String(req.OtherPublicKey);
                var encryptedBytes = Convert.FromBase64String(req.Message);
                var decrypted = _ecc.Decrypt(encryptedBytes, myPrivateKeyBytes, otherPublicKeyBytes);
                return Ok(decrypted);
            }
            catch 
            { 
                return BadRequest("Decryption failed. Check keys."); 
            }
        }
        
        [HttpPost("analyze")]
        public IActionResult Analyze([FromBody] ECCRequest req)
        {
            // This method now returns a value on all paths.
            return Ok(_ai.AnalyzeMessage(req.Message));
        }

        [HttpPost("processhistory")]
        public IActionResult ProcessHistory([FromBody] HistoryRequest req, [FromQuery] string action)
        {
            try
            {
                var myPrivateKeyBytes = Convert.FromBase64String(req.MyPrivateKey);
                var otherPublicKeyBytes = Convert.FromBase64String(req.OtherPublicKey);

                if (action == "encrypt")
                {
                    var encryptedHistory = _ecc.Encrypt(req.HistoryJson, myPrivateKeyBytes, otherPublicKeyBytes);
                    return Ok(Convert.ToBase64String(encryptedHistory));
                }
                if (action == "decrypt")
                {
                    var encryptedHistoryBytes = Convert.FromBase64String(req.HistoryJson);
                    var decryptedHistory = _ecc.Decrypt(encryptedHistoryBytes, myPrivateKeyBytes, otherPublicKeyBytes);
                    return Ok(decryptedHistory);
                }
                return BadRequest("Invalid action specified.");
            }
            catch
            {
                return BadRequest("Failed to process history. The private key may be incorrect for this session.");
            }
        }
    }
}