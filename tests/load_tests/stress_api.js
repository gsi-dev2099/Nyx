import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '10s', target: 50 }, // Ramp-up a 50 usuarios concurrentes en 10s
    { duration: '30s', target: 50 }, // Mantener 50 usuarios concurrentes por 30s
    { duration: '10s', target: 0 },  // Ramp-down a 0 usuarios en 10s
  ],
  thresholds: {
    // El 95% de las peticiones deben ser menores a 200ms
    http_req_duration: ['p(95)<200'],
    // La tasa de fallos debe ser menor al 1% (como acordado para compensar la carga del entorno local)
    http_req_failed: ['rate<0.01'],
  },
};

export default function () {
  // Apuntamos al servicio dentro de la red de docker 'nyx_default'
  const res = http.get('http://crm_apihub:5068/api/health');
  
  check(res, {
    'status is 200': (r) => r.status === 200,
    'status is not degraded': (r) => r.status !== 503,
  });

  sleep(1);
}
