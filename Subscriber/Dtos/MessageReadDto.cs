﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subscriber.Dtos
{
    public class MessageReadDto
    {
        public int Id { get; set; }
        public string? TopicMessage { get; set; }
        public string? MessageStatus { get; set; }
        public DateTime ExpiresAfter { get; set; }

    }
}
