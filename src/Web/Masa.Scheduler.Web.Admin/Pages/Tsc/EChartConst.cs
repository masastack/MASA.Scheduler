// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Pages.Tsc;

internal static class EChartConst
{
    #region init json
    private const string BarBasicJson = @"{
 ""tooltip"":{""trigger"":""axis"",""show"":true},
 ""legend"":{""show"":true,""left"":""center"",""top"":""top""},
  ""grid"": {
    ""show"": false,
    ""top"": 20,
    ""bottom"": 20,
    ""left"": 40,
    ""right"": 20
  },
""xAxis"": {
    ""type"": ""category""
  },
  ""yAxis"": {
    ""type"": ""value""
  },
  ""series"": [
    {
      ""name"":""2022"",
      ""data"": [120, 200, 150, 80, 70, 110, 130],
      ""type"": ""bar""
    }
  ]
}";
    private const string LineBasicJson = @"{
 ""tooltip"":{""trigger"":""axis"",""show"":true},
 ""legend"":{""show"":true,""left"":""center"",""top"":""top""},
 ""grid"": {
    ""show"": false,
    ""top"": 20,
    ""bottom"": 20,
    ""left"": 40,
    ""right"": 20
  },
  ""xAxis"": {
    ""type"": ""category""
  },
  ""yAxis"": {
    ""type"": ""value""
  },
  ""series"": [
    {
      ""data"": [150, 230, 224, 218, 135, 147, 260],
      ""type"": ""line""
    }
  ]
}";
    private const string LineAreaBasicJson = @"{
  ""tooltip"": {
    ""trigger"": ""axis"",
    ""axisPointer"": {
      ""type"": ""cross"",
      ""label"": {
        ""backgroundColor"": ""#6a7985""
      }
    }
  },
  ""legend"":{""show"":true,""left"":""center"",""top"":""top""},
  ""grid"": {
    ""left"": ""3%"",
    ""right"": ""4%"",
    ""bottom"": ""3%"",
    ""containLabel"": true
  },
  ""xAxis"": [
    {
      ""type"": ""category"",
      ""boundaryGap"": false
    }
  ],
  ""yAxis"": [
    {
      ""type"": ""value""
    }
  ],
  ""series"": [
    {
      ""name"": ""Email"",
      ""type"": ""line"",
      ""stack"": ""Total"",
      ""areaStyle"": {},
      ""emphasis"": {
        ""focus"": ""series""
      },
      ""data"": [120, 132, 101, 134, 90, 230, 210]
    },
    {
      ""name"": ""Union Ads"",
      ""type"": ""line"",
      ""stack"": ""Total"",
      ""areaStyle"": {},
      ""emphasis"": {
        ""focus"": ""series""
      },
      ""data"": [220, 182, 191, 234, 290, 330, 310]
    },
    {
      ""name"": ""Video Ads"",
      ""type"": ""line"",
      ""stack"": ""Total"",
      ""areaStyle"": {},
      ""emphasis"": {
        ""focus"": ""series""
      },
      ""data"": [150, 232, 201, 154, 190, 330, 410]
    },
    {
      ""name"": ""Direct"",
      ""type"": ""line"",
      ""stack"": ""Total"",
      ""areaStyle"": {},
      ""emphasis"": {
        ""focus"": ""series""
      },
      ""data"": [320, 332, 301, 334, 390, 330, 320]
    },
    {
      ""name"": ""Search Engine"",
      ""type"": ""line"",
      ""stack"": ""Total"",
      ""label"": {
        ""show"": true,
        ""position"": ""top""
      },
      ""areaStyle"": {},
      ""emphasis"": {
        ""focus"": ""series""
      },
      ""data"": [820, 932, 901, 934, 1290, 1330, 1320]
    }
  ]
}";
    private const string PieBasicJson = @"{
  ""tooltip"": {
    ""trigger"": ""item""
  },
  ""legend"": {
    ""orient"": ""vertical"",
    ""left"": ""left""
  },
  ""series"": [
    {
      ""type"": ""pie"",
      ""radius"": ""50%"",
      ""data"": [
        { ""value"": 1048, ""name"": ""Search Engine"" },
        { ""value"": 735, ""name"": ""Direct"" },
        { ""value"": 580, ""name"": ""Email"" },
        { ""value"": 484, ""name"": ""Union Ads"" },
        { ""value"": 300, ""name"": ""Video Ads"" }
      ],
      ""emphasis"": {
        ""itemStyle"": {
          ""shadowBlur"": 10,
          ""shadowOffsetX"": 0,
          ""shadowColor"": ""rgba(0, 0, 0, 0.5)""
        }
      }
    }
  ]
}";
    private const string GaugeBasicJson = @"{
  ""tooltip"": {
    ""trigger"": ""item""
  },
  ""series"": [
    {
      ""type"": ""gauge"",
      ""anchor"": {
        ""show"": true,
        ""showAbove"": true,
        ""size"": 18,
        ""itemStyle"": {
          ""color"": ""#FAC858""
        }
      },
    ""pointer"": {
        ""icon"": ""path://M2.9,0.7L2.9,0.7c1.4,0,2.6,1.2,2.6,2.6v115c0,1.4-1.2,2.6-2.6,2.6l0,0c-1.4,0-2.6-1.2-2.6-2.6V3.3C0.3,1.9,1.4,0.7,2.9,0.7z"",
       ""width"": 8,
        ""length"": ""80%"",
        ""offsetCenter"": [0, ""8%""]
      },
    ""progress"": {
        ""show"": true,
        ""overlap"": true,
        ""roundCap"": true
      },
    ""axisLine"": {
        ""roundCap"": true
      },
    ""title"": {
        ""fontSize"": 14
      },
      ""detail"": {
        ""width"": 40,
        ""height"": 14,
        ""fontSize"": 14,
        ""color"": ""#fff"",
        ""backgroundColor"": ""auto"",
        ""borderRadius"": 3,
        ""formatter"": ""{value}""
      },
      ""data"": [
        {
          ""value"": 50,
          ""name"": ""SCORE""
        }
      ]
    }
  ]
}";
    private const string HeatmapBasicJson = @"{
    ""tooltip"": {
        ""position"": ""top""
    },
    ""grid"": {
        ""height"": ""50%"",
        ""top"": ""10%""
    },
    ""xAxis"": {
        ""type"": ""category"",
        ""data"": [
            ""12a"",
            ""1a"",
            ""2a"",
            ""3a"",
            ""4a"",
            ""5a"",
            ""6a"",
            ""7a"",
            ""8a"",
            ""9a"",
            ""10a"",
            ""11a"",
            ""12p"",
            ""1a"",
            ""2a"",
            ""3a""
        ],
        ""splitArea"": {
            ""show"": true
        }
    },
    ""yAxis"": {
        ""type"": ""category"",
        ""data"": [
            ""Saturday"",
            ""Friday"",
            ""Thursday"",
            ""Wednesday"",
            ""Tuesday"",
            ""Monday"",
            ""Sunday""
        ],
        ""splitArea"": {
            ""show"": true
        }
    },
    ""visualMap"": {
        ""min"": 0,
        ""max"": 10,
        ""calculable"": true,
        ""orient"": ""horizontal"",
        ""left"": ""center"",
        ""bottom"": ""5%""
    },
    ""series"": [
        {
            ""name"": ""Punch Card"",
            ""type"": ""heatmap"",
            ""data"": [
                [
                    8,
                    0,
                    0
                ],
                [
                    9,
                    0,
                    0
                ],
                [
                    10,
                    0,
                    0
                ],
                [
                    11,
                    0,
                    2
                ],
                [
                    15,
                    0,
                    3
                ],
                [
                    16,
                    0,
                    4
                ],
                [
                    17,
                    0,
                    6
                ],
                [
                    18,
                    0,
                    4
                ],
                [
                    19,
                    0,
                    4
                ],
                [
                    20,
                    0,
                    3
                ],
                [
                    21,
                    0,
                    3
                ],
                [
                    22,
                    0,
                    2
                ],
                [
                    23,
                    0,
                    5
                ],
                [
                    0,
                    1,
                    7
                ],
                [
                    1,
                    1,
                    0
                ],
                [
                    2,
                    1,
                    0
                ],
                [
                    3,
                    1,
                    0
                ],
                [
                    4,
                    1,
                    0
                ],
                [
                    5,
                    1,
                    0
                ],
                [
                    6,
                    1,
                    0
                ],
                [
                    7,
                    1,
                    0
                ],
                [
                    8,
                    1,
                    0
                ],
                [
                    9,
                    1,
                    0
                ],
                [
                    10,
                    1,
                    5
                ],
                [
                    11,
                    1,
                    2
                ],
                [
                    12,
                    1,
                    2
                ],
                [
                    13,
                    1,
                    6
                ],
                [
                    14,
                    1,
                    9
                ],
                [
                    15,
                    1,
                    11
                ],
                [
                    16,
                    1,
                    6
                ],
                [
                    17,
                    1,
                    7
                ],
                [
                    18,
                    1,
                    8
                ],
                [
                    19,
                    1,
                    12
                ],
                [
                    20,
                    1,
                    5
                ],
                [
                    21,
                    1,
                    5
                ],
                [
                    22,
                    1,
                    7
                ],
                [
                    23,
                    1,
                    2
                ],
                [
                    0,
                    2,
                    1
                ],
                [
                    1,
                    2,
                    1
                ],
                [
                    2,
                    2,
                    0
                ],
                [
                    3,
                    2,
                    0
                ],
                [
                    4,
                    2,
                    0
                ],
                [
                    5,
                    2,
                    0
                ],
                [
                    6,
                    2,
                    0
                ],
                [
                    9,
                    2,
                    0
                ],
                [
                    10,
                    2,
                    3
                ],
                [
                    16,
                    2,
                    6
                ],
                [
                    4,
                    4,
                    0
                ],
                [
                    5,
                    4,
                    1
                ],
                [
                    6,
                    4,
                    0
                ],
                [
                    7,
                    4,
                    0
                ],
                [
                    8,
                    4,
                    0
                ],
                [
                    9,
                    4,
                    2
                ],
                [
                    10,
                    4,
                    4
                ],
                [
                    11,
                    4,
                    4
                ],
                [
                    22,
                    4,
                    3
                ],
                [
                    23,
                    4,
                    0
                ],
                [
                    0,
                    5,
                    2
                ],
                [
                    1,
                    5,
                    1
                ],
                [
                    2,
                    5,
                    0
                ],
                [
                    3,
                    5,
                    3
                ],
                [
                    4,
                    5,
                    0
                ],
                [
                    5,
                    5,
                    0
                ],
                [
                    6,
                    5,
                    0
                ],
                [
                    7,
                    5,
                    0
                ],
                [
                    8,
                    5,
                    2
                ],
                [
                    9,
                    5,
                    0 
                ],
                [
                    10,
                    5,
                    4
                ],
                [
                    11,
                    5,
                    1
                ],
                [
                    12,
                    5,
                    5
                ],
                [
                    13,
                    5,
                    10
                ],
                [
                    14,
                    5,
                    5
                ],
                [
                    15,
                    5,
                    7
                ],
                [
                    16,
                    5,
                    11
                ],
                [
                    17,
                    5,
                    6
                ],
                [
                    18,
                    5,
                    0
                ],
                [
                    19,
                    5,
                    5
                ],
                [
                    20,
                    5,
                    3
                ],
                [
                    21,
                    5,
                    4
                ],
                [
                    22,
                    5,
                    2
                ],
                [
                    23,
                    5,
                    0
                ],
                [
                    0,
                    6,
                    1
                ],
                [
                    1,
                    6,
                    0
                ]
            ],
            ""label"": {
                ""show"": true
            },
            ""emphasis"": {
                ""itemStyle"": {
                    ""shadowBlur"": 10,
                    ""shadowColor"": ""rgba(0, 0, 0, 0.5)""
                }
            }
        }
    ]
}";
    #endregion

    public static EChartType Bar
    {
        get
        {
            return new EChartType("bar", "", BarBasicJson);
        }
    }
    public static EChartType Pie
    {
        get
        {
            return new EChartType("pie", "", PieBasicJson);
        }
    }
    public static EChartType Line
    {
        get
        {
            return new EChartType("line", "", LineBasicJson);
        }
    }
    public static EChartType Gauge
    {
        get
        {
            return new EChartType("gauge", "", GaugeBasicJson);
        }
    }
    public static EChartType Heatmap
    {
        get
        {
            return new EChartType("heatmap", "", HeatmapBasicJson);
        }
    }
    public static EChartType LineArea
    {
        get
        {
            return new EChartType("line-area", "", LineAreaBasicJson);
        }
    }
}

public class EChartType
{
    public EChartType(string name, string src, string json)
    {
        Name = name;
        Src = src;
        Json = JsonNode.Parse(Regex.Replace(json, @"\s", ""))!;
    }

    public string Name { get; set; }

    public string Src { get; set; }

    [JsonIgnore]
    public object Option => Json.DeepClone();

    public JsonNode Json { get; set; }

    public void SetValue(string path, object value)
    {
        if (string.IsNullOrEmpty(path))
            return;
        var paths = path.Split('.');
        var target = ConvertJsonNode(value);

        int pathIndex = 0;
        var current = Json;
        do
        {
            current = SetAttr(paths[pathIndex++], current, pathIndex - paths.Length == 0 ? target : null);
            var isLastPath = pathIndex - paths.Length == 0;
            if (isLastPath)
                break;
        }
        while (true);
    }

    private JsonNode SetAttr(string name, JsonNode source, JsonNode? value)
    {
        if (source is JsonArray)
            source = NewObject();

        bool isArray = GetNameArrayAttr(name, out string newName, out int[] arrayLen);
        if (isArray)
        {
            var ddd = source[newName];
            if (ddd == null)
                ddd = NewArray(arrayLen);
            else
            {
                try
                {
                    var array = ddd.AsArray();
                    SetArrayLen(array, arrayLen);
                    ddd = array;
                }
                catch
                {
                    ddd = NewArray(arrayLen);
                }
            }
            source[newName] = ddd;
            return SetArrayValue(ddd.AsArray(), arrayLen, value);
        }
        else
        {
            if (source[name] == null)
                source[name] = NewObject();
            if (value != null)
                source[name] = value;
            else if (source[name] is JsonArray)
                source[name] = NewObject();
            return source[name]!;
        }
    }

    private void SetArrayLen(JsonArray json, int[] len)
    {
        if (len.Length == 0)
            return;

        var count = len[0] + 1 - json.Count;
        if (count > 0)
        {
            do
            {
                json.Add(NewArray(len[1..]));
            }
            while (--count > 0);
        }

        if (len.Length - 1 > 0)
            foreach (var item in json)
            {
                if (item == null)
                    continue;
                SetArrayLen(item.AsArray(), len[1..]);
            }
    }

    private JsonObject NewObject() => new();

    private JsonArray NewArray(params int[] len)
    {
        if (len.Length == 0) return new();

        var array = new JsonArray();
        var index = len[0] + 1;

        while (index-- > 0)
            array.Add(null);

        if (len.Length > 1)
        {
            array[len[0]] = NewArray(len[1..]);
        }
        else
        {
            if (array[len[0]] == null)
                array[len[0]] = NewObject();
        }

        return array;
    }

    private JsonNode SetArrayValue(JsonArray array, int[] len, JsonNode? value)
    {
        var index = 0;
        do
        {
            var find = array[len[index]]!;
            if (index - len.Length + 1 == 0)
            {
                if (value != null)
                {
                    find = value;
                    array[len[index]] = find;
                }
                return find;
            }
            index++;
            array = find.AsArray();
        }
        while (true);
    }

    private JsonNode ConvertJsonNode(object value)
    {
        var text = JsonSerializer.Serialize(value);
        return JsonNode.Parse(text)!;
    }

    /// <summary>
    /// name[0][1] return name int[]{0,1},name[3] return name int[]{3},name return name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="newName"></param>
    /// <param name="indexes"></param>
    /// <returns></returns>
    private bool GetNameArrayAttr(string name, out string newName, out int[] indexes)
    {
        indexes = default!;
        newName = default!;
        var matches = Regex.Matches(name, @"\[\d+\]");
        if (!matches.Any(m => m.Success))
            return false;

        newName = Regex.Replace(name, @"\[\d+\]", "");
        indexes = matches.Where(m => m.Success).Select(m => Convert.ToInt32(m.Value[1..(m.Value.Length - 1)])).ToArray();

        return true;
    }

    public string GetValue()
    {
        return Json.ToJsonString();
    }
}