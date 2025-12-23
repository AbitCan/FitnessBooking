const USERS_KEY = "fb_users";
const TOKEN_KEY = "fb_token";

function loadUsers() {
  return JSON.parse(localStorage.getItem(USERS_KEY) || "[]");
}
function saveUsers(users) {
  localStorage.setItem(USERS_KEY, JSON.stringify(users));
}

document.getElementById("signupForm").addEventListener("submit", (e) => {
  e.preventDefault();
  const email = document.getElementById("signupEmail").value.trim().toLowerCase();
  const pass = document.getElementById("signupPass").value;

  const out = document.getElementById("signupOut");
  const users = loadUsers();

  if (users.some(u => u.email === email)) {
    out.textContent = "User already exists.";
    return;
  }

  users.push({ email, pass }); // demo only (plaintext, not secure)
  saveUsers(users);
  out.textContent = "Account created. You can login now.";
});

document.getElementById("loginForm").addEventListener("submit", (e) => {
  e.preventDefault();
  const email = document.getElementById("loginEmail").value.trim().toLowerCase();
  const pass = document.getElementById("loginPass").value;

  const out = document.getElementById("loginOut");
  const users = loadUsers();

  const ok = users.some(u => u.email === email && u.pass === pass);
  if (!ok) {
    out.textContent = "Invalid credentials.";
    return;
  }

  localStorage.setItem(TOKEN_KEY, "demo-token-" + Date.now());
  window.location.href = "/";
});
