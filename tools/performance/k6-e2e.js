import http from "k6/http";
import { check, sleep } from "k6";

export const options = {
  scenarios: {
    e2e: {
      executor: "constant-vus",
      vus: 5,
      duration: "30s",
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.01"],

    // Overall latency targets
    http_req_duration: ["p(95)<800"],

    // Per-endpoint latency targets (tagged)
    "http_req_duration{endpoint:classes}": ["p(95)<800"],
    "http_req_duration{endpoint:members}": ["p(95)<800"],
    "http_req_duration{endpoint:reservations_create}": ["p(95)<800"],
    "http_req_duration{endpoint:reservations_cancel}": ["p(95)<800"],
  },
};

function mustEnv(name) {
  const v = __ENV[name];
  if (!v) throw new Error(`Missing env var ${name}. Example: -e ${name}=http://localhost:5039`);
  return v.replace(/\/+$/, "");
}

export default function () {
  const BASE_URL = mustEnv("BASE_URL");

  // ---------- POST /classes ----------
  const classBody = JSON.stringify({
    name: `Refund Demo ${__VU}-${__ITER}`,
    instructor: "Alex",
    capacity: 10,
    startAtUtc: "2030-01-01T12:00:00Z",
  });

  const resClass = http.post(`${BASE_URL}/classes`, classBody, {
    headers: { "Content-Type": "application/json" },
    tags: { endpoint: "classes" },
  });

  check(resClass, { "classes: 201": (r) => r.status === 201 });

  const classJson = safeJson(resClass);
  const classId = classJson?.id;

  // ---------- POST /members ----------
  // IMPORTANT: adjust membershipType if your enum differs:
  // Standard=0, Premium=1, Student=2 (common)
  const memberBody = JSON.stringify({
    name: `User ${__VU}-${__ITER}`,
    membershipType: 0,
  });

  const resMember = http.post(`${BASE_URL}/members`, memberBody, {
    headers: { "Content-Type": "application/json" },
    tags: { endpoint: "members" },
  });

  check(resMember, { "members: 201": (r) => r.status === 201 });

  const memberJson = safeJson(resMember);
  const memberId = memberJson?.id;

  // ---------- POST /reservations ----------
  const reservationBody = JSON.stringify({
    memberId,
    classId,
  });

  const resReservation = http.post(`${BASE_URL}/reservations`, reservationBody, {
    headers: { "Content-Type": "application/json" },
    tags: { endpoint: "reservations_create" },
  });

  check(resReservation, { "reservations create: 201": (r) => r.status === 201 });

  const reservationJson = safeJson(resReservation);
  const reservationId = reservationJson?.id;

  // ---------- DELETE /reservations/{id} ----------
  const resCancel = http.del(`${BASE_URL}/reservations/${reservationId}`, null, {
    tags: { endpoint: "reservations_cancel" },
  });

  check(resCancel, {
    "cancel: 200/204": (r) => r.status === 200 || r.status === 204,
  });

  sleep(0.2);
}

function safeJson(res) {
  try {
    return res.json();
  } catch {
    return null;
  }
}
