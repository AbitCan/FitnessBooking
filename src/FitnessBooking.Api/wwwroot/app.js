const $ = (id) => document.getElementById(id);

function setOut(el, objOrText) {
  if (typeof objOrText === "string") el.textContent = objOrText;
  else el.textContent = JSON.stringify(objOrText, null, 2);
}

async function readBodySafe(resp) {
  const text = await resp.text();
  try { return { kind: "json", value: JSON.parse(text) }; }
  catch { return { kind: "text", value: text }; }
}

async function api(path, options) {
  const resp = await fetch(path, {
    headers: { "Content-Type": "application/json" },
    ...options
  });

  const body = await readBodySafe(resp);
  return { resp, body };
}

let state = {
  classId: null,
  memberId: null,
  reservationId: null
};

function syncInputs() {
  if (state.classId) {
    $("classId").textContent = state.classId;
    if (!$("reservationClassId").value) $("reservationClassId").value = state.classId;
  }
  if (state.memberId) {
    $("memberId").textContent = state.memberId;
    if (!$("reservationMemberId").value) $("reservationMemberId").value = state.memberId;
  }
  if (state.reservationId) {
    $("reservationId").textContent = state.reservationId;
    if (!$("cancelReservationId").value) $("cancelReservationId").value = state.reservationId;
  }
}

$("form-class").addEventListener("submit", async (e) => {
  e.preventDefault();

  const payload = {
    name: $("className").value.trim(),
    instructor: $("classInstructor").value.trim(),
    capacity: Number($("classCapacity").value),
    startAtUtc: $("classStartUtc").value.trim()
  };

  const { resp, body } = await api("/classes", { method: "POST", body: JSON.stringify(payload) });

  setOut($("out-class"), { status: resp.status, body: body.value });

  if (resp.ok && body.kind === "json" && body.value?.id) {
    state.classId = body.value.id;
    syncInputs();
  }
});

$("form-member").addEventListener("submit", async (e) => {
  e.preventDefault();

  const payload = {
    name: $("memberName").value.trim(),
    membershipType: Number($("memberType").value)
  };

  const { resp, body } = await api("/members", { method: "POST", body: JSON.stringify(payload) });

  setOut($("out-member"), { status: resp.status, body: body.value });

  if (resp.ok && body.kind === "json" && body.value?.id) {
    state.memberId = body.value.id;
    syncInputs();
  }
});

$("form-reservation").addEventListener("submit", async (e) => {
  e.preventDefault();

  const memberId = $("reservationMemberId").value.trim() || state.memberId;
  const classId = $("reservationClassId").value.trim() || state.classId;

  const payload = { memberId, classId };

  const { resp, body } = await api("/reservations", { method: "POST", body: JSON.stringify(payload) });

  setOut($("out-reservation"), { status: resp.status, body: body.value });

  if (resp.ok && body.kind === "json" && body.value?.id) {
    state.reservationId = body.value.id;
    syncInputs();
  }
});

$("form-cancel").addEventListener("submit", async (e) => {
  e.preventDefault();

  const id = $("cancelReservationId").value.trim() || state.reservationId;
  const cancelUtc = $("cancelUtc").value.trim();

  if (!id) {
    setOut($("out-cancel"), "Missing reservationId");
    return;
  }

  // If your API expects cancelUtc in query string, this line matches that style:
  // DELETE /reservations/{id}?cancelUtc=...
  const url = `/reservations/${encodeURIComponent(id)}?cancelUtc=${encodeURIComponent(cancelUtc)}`;

  const { resp, body } = await api(url, { method: "DELETE" });

  setOut($("out-cancel"), { status: resp.status, body: body.value });

  if (resp.ok) {
    // Your API returns JSON like { refund: 200 } (based on your earlier outputs)
    if (body.kind === "json" && body.value?.refund !== undefined) {
      $("refund").textContent = String(body.value.refund);
    } else {
      $("refund").textContent = "(see output)";
    }
  }
});

// Initial paint
syncInputs();
