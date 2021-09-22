﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tilde.MT.TranslationAPIService.Models
{
    public class Translation
    {
        /// <summary>
        /// The text domain of the translation system used to produce the translation. This property contain automatically detected domain if not specified within the request.
        /// </summary>
        public string Domain { get; set; }
        /// <summary>
        /// Translation results
        /// </summary>
        public List<TranslationItem> Translations { get; set; }
    }
}
