﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace API.Models
{
  public class LinkedInUserEmailModel
  {
    public IEnumerable<Element> Elements { get; set; } = null!;
  }

  public class Element
  {
    [JsonProperty("handle~")]
    public Handle Handle { get; set; } = null!;
  }

  public class Handle
  {
    public string EmailAddress { get; set; } = null!;
  }
}
