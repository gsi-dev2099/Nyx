import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 50 }, // Ramp-up a 50 usuarios
    { duration: '1m', target: 50 },  // Mantener carga
    { duration: '30s', target: 0 },  // Ramp-down
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% de las peticiones deben ser menores a 500ms
    http_req_failed: ['rate<0.01'],   // Menos de 1% de errores
  },
};

const BASE_URL = 'http://localhost:5068/api';

export default function () {
  // 1. Prueba de Login
  const loginPayload = JSON.stringify({
    username: 'admin',
    password: 'password123',
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  const loginRes = http.post(`${BASE_URL}/auth/login`, loginPayload, params);
  
  check(loginRes, {
    'login exitoso (200)': (r) => r.status === 200,
    'login fallido por rate limit (429)': (r) => r.status === 429,
  });

  if (loginRes.status === 200) {
    const tokens = loginRes.json();
    const refreshToken = tokens.refreshToken;

    sleep(1); // Simular tiempo de trabajo

    // 2. Prueba de Refresh Token
    const refreshPayload = JSON.stringify({
      refreshToken: refreshToken,
    });

    const refreshRes = http.post(`${BASE_URL}/auth/refresh`, refreshPayload, params);
    check(refreshRes, {
      'refresh exitoso (200)': (r) => r.status === 200,
    });
  } else {
    sleep(1); // Backoff si recibe 429
  }
}
