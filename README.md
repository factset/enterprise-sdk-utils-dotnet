<img alt="FactSet" src="https://www.factset.com/hubfs/Assets/images/factset-logo.svg" height="56" width="290">

# FactSet SDK Utilities for .NET

[![Nuget](https://img.shields.io/nuget/v/FactSet.SDK.Utils)](https://www.nuget.org/packages/FactSet.SDK.Utils)
[![Apache-2 license](https://img.shields.io/badge/license-Apache2-brightgreen.svg)](https://www.apache.org/licenses/LICENSE-2.0)

This repository contains a collection of utilities that supports FactSet's SDK in .NET and facilitate usage of FactSet APIs.

## Installation

### .NET CLI

```bash
dotnet add package FactSet.SDK.Utils
```

### NuGet

```bash
nuget install FactSet.SDK.Utils
```

## Usage

This library contains multiple modules, sample usage of each module is below.

### Authentication

First, you need to create the OAuth 2.0 client configuration that will be used to authenticate against FactSet's APIs:

1. Create a [new application](https://developer.factset.com/applications) on FactSet's Developer Portal.
2. When prompted, download the configuration file and move it to your development environment.

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FactSet.SDK.Utils.Authentication;

namespace Console
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            ConfidentialClient confidentialClient = await ConfidentialClient.CreateAsync("./path/to/config.json");
            string token = await confidentialClient.GetAccessTokenAsync();

            HttpClient client = new HttpClient();
            HttpResponseMessage res;
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get,
                                                               "https://api.factset.com/analytics/lookups/v3/currencies"))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                res = await client.SendAsync(requestMessage);
            }

            Console.WriteLine(await res.Content.ReadAsStringAsync());
        }
    }
}
```

## Modules

Information about the various utility modules contained in this library can be found below.

### Authentication

The [authentication module](src/FactSet.SDK.Utils/Authentication) provides helper classes that facilitate [OAuth 2.0](https://github.com/factset/oauth2-guidelines) authentication and authorization with FactSet's APIs. Currently the module has support for the [client credentials flow](https://github.com/factset/oauth2-guidelines#client-credentials-flow-1).

Each helper class in the module has the following features:

* Accepts a `Configuration` instance that contains information about the OAuth 2.0 client, including the client ID and private key.
* Performs authentication with FactSet's OAuth 2.0 authorization server and retrieves an access token.
* Caches the access token for reuse and requests a new access token as needed when one expires.

#### Configuration

Classes in the authentication module require OAuth 2.0 client configuration information to be passed to the `CreateAsync` factory method in the `ConfidentialClient` through a JSON-formatted file or a `Configuration` object. Below is an example of a JSON-formatted file:

```json
{
    "name": "Application name registered with FactSet's Developer Portal",
    "clientId": "OAuth 2.0 Client ID registered with FactSet's Developer Portal",
    "clientAuthType": "Confidential",
    "owners": ["USERNAME-SERIAL"],
    "jwk": {
        "kty": "RSA",
        "use": "sig",
        "alg": "RS256",
        "kid": "Key ID",
        "d": "ECC Private Key",
        "n": "Modulus",
        "e": "Exponent",
        "p": "First Prime Factor",
        "q": "Second Prime Factor",
        "dp": "First Factor CRT Exponent",
        "dq": "Second Factor CRT Exponent",
        "qi": "First CRT Coefficient"
    }
}
```

The other option is to pass in the `Configuration` instance which is initialised as shown below:

```csharp
using System.Collections.Generic;
using FactSet.SDK.Utils.Authentication;
using Microsoft.IdentityModel.Tokens;

// Fill values with values from your generated config.
var jwk = new JsonWebKey(@"
    {
        'kty': 'RSA',
        'use': 'sig',
        'alg': 'RS256',
        'kid': 'Key ID',
        'd': 'ECC Private Key',
        'n': 'Modulus',
        'e': 'AQAB',
        'p': 'First Prime Factor',
        'q': 'Second Prime Factor',
        'dp': 'First Factor CRT Exponent',
        'dq': 'Second Factor CRT Exponent',
        'qi': 'First CRT Coefficient'
    }");

var config = new Configuration("client id", "Confidential", jwk);
```

If you're just starting out, you can visit FactSet's Developer Portal to [create a new application](https://developer.factset.com/applications) and download a configuration file in this format.

If you're creating and managing your signing key pair yourself, see the required [JWK parameters](https://github.com/factset/oauth2-guidelines#jwk-parameters) for public-private key pairs.

## Debugging

This library uses the built-in [Trace class](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.trace?view=net-5.0) to log various messages that will help you understand what it's doing. For more information on how to redirect the log output or change the log message severity, read the documentation linked.

# Contributing

Please refer to the [contributing guide](CONTRIBUTING.md).

# Copyright

Copyright 2021 FactSet Research Systems Inc

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
