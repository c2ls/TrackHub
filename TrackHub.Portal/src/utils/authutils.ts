/**
* Copyright (c) 2025 Sergio Hernandez. All rights reserved.
*
*  Licensed under the Apache License, Version 2.0 (the "License").
*  You may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License.
*/

import SHA256 from 'crypto-js/sha256';
import Base64 from 'crypto-js/enc-base64';

const generateCodeVerifier = (): string => {
    const charset =
      'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~';
    let verifier = '';
    for (let i = 0; i < 128; i++) {
      const randomIndex = Math.floor(Math.random() * charset.length);
      verifier += charset[randomIndex];
    }
    return verifier;
  };

const generateCodeChallenge = (plain: string): string => {
    const hash = SHA256(plain).toString(Base64);
    return hash.replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
  };

export {
    generateCodeVerifier,
    generateCodeChallenge
};
