// ===== helpers =====
const $ = (s) => document.querySelector(s);
const inputs = [
  'distanceKm','durationMinutes','publicCarFarePerRide','publicCarRidesCount',
  'uberBaseFare','uberPerKm','uberPerMinute','uberServiceFee','uberSurgeMultiplier',
  'fuelPricePerGallon','carEfficiencyKmPerGallon','tolls','parking','maintenancePerKm'
];
const ls = {
  get:(k,d)=>{try{return JSON.parse(localStorage.getItem(k))??d;}catch{ return d; }},
  set:(k,v)=>localStorage.setItem(k,JSON.stringify(v)),
  del:(k)=>localStorage.removeItem(k)
};
function getApiBase(){ return ($('#apiBase')?.value?.trim()) || 'https://localhost:7140'; }
function getEndpoint(){ return `${getApiBase()}/api/Costs/calculate`; }
function getCurrency(){ return $('#currency')?.value || 'DOP'; }
function setCurrency(c){ const el=$('#currency'); if(el) el.value=c; }
function applyDark(on){ document.body.classList.toggle('dark', !!on); $('#darkMode').checked=!!on; ls.set('rc_dark',!!on); }
function fmtMoney(v){ try{ return new Intl.NumberFormat('es-DO',{style:'currency',currency:getCurrency(),minimumFractionDigits:2}).format(v??0);}catch{ return `${getCurrency()} ${Number(v??0).toFixed(2)}`; } }
function num(id, def=0){ const el=$(`#${id}`); const n=Number(el?.value?.toString().trim()); return Number.isFinite(n)&&n>=0 ? n : def; }
function readPayload(){ const o={}; for(const id of inputs) o[id]=num(id,0); return o; }
function setStatus(msg, error=false, loading=false){
  const box=$('#status'); if(!box) return;
  box.style.display='flex'; box.style.alignItems='center'; box.style.gap='8px';
  box.style.background= error ? '#ffe8e8' : '#f8fafc';
  box.style.color= error ? '#7f1d1d' : '#374151';
  box.innerHTML = `${loading?'<span class="spinner"></span>':''}<span>${msg}</span>`;
}
function clearStatus(){ const b=$('#status'); if(b){ b.style.display='none'; b.textContent=''; } }
function escapeHtml(s=''){ return String(s).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;').replace(/'/g,'&#039;'); }
function round2(x){ return Math.round((x??0)*100)/100; }
function validate(){
  const bad=[]; for(const id of inputs){ const el=$(`#${id}`); const val=Number(el.value); const ok=Number.isFinite(val)&&val>=0; el.setCustomValidity(ok?'':'Valor inválido'); if(!ok) bad.push(id); }
  return { ok: bad.length===0, bad };
}
function saveForm(){
  const vals={}; for(const id of inputs) vals[id]=$(`#${id}`)?.value ?? '';
  ls.set('rc_formValues', vals);
  ls.set('rc_apiBase', $('#apiBase')?.value ?? '');
  ls.set('rc_autoCalc', $('#autoCalc')?.checked ?? false);
  ls.set('rc_currency', getCurrency());
  ls.set('rc_note', $('#note')?.value ?? '');
}
function loadForm(){
  const vals=ls.get('rc_formValues',null); if(vals) for(const id of inputs){ const el=$(`#${id}`); if(el&&vals[id]!=null) el.value=vals[id]; }
  const api=ls.get('rc_apiBase',null); if(api) $('#apiBase').value=api;
  const auto=!!ls.get('rc_autoCalc',false); $('#autoCalc').checked=auto;
  const cur=ls.get('rc_currency','DOP'); setCurrency(cur);
  const note=ls.get('rc_note',''); if($('#note')) $('#note').value=note;
  applyDark(ls.get('rc_dark',false));
}

// ===== render =====
const modeLabel = (m)=>({PublicCar:'Carro público',OwnCar:'Carro propio',Uber:'Uber'}[m]??m);
function renderResult(data){
  const container=$('#result'); if(!container) return;
  if(!data || !Array.isArray(data.options) || !data.options.length){ container.innerHTML='<div class="muted">No hay datos.</div>'; return; }
  const options=[...data.options].sort((a,b)=>(a.totalCost??0)-(b.totalCost??0));
  const best=options[0]; const max=Math.max(...options.map(o=>o.totalCost??0))||1;

  const bestHtml = `
    <div class="option best">
      <div class="row" style="justify-content: space-between;">
        <div><span class="badge">Más económico</span></div>
        <div class="price">${fmtMoney(round2(best.totalCost))}</div>
      </div>
      <div class="mode">${escapeHtml(modeLabel(best.mode))}</div>
      <div class="muted">${escapeHtml(best.breakdown||'')}</div>
      <div class="bar"><span style="width:${(best.totalCost/max)*100}%;"></span></div>
    </div>`;

  const listHtml = options.map((o,i)=>{
    const diff=round2((o.totalCost??0)-(best.totalCost??0));
    const tag = i===0 ? 'Más económico' : (diff>0?`+${fmtMoney(diff)} vs. mejor`:'Igual que el mejor');
    return `
      <div class="option ${i===0?'best':''}">
        <div class="row" style="justify-content: space-between;">
          <div class="mode">${escapeHtml(modeLabel(o.mode))}</div>
          <div class="price">${fmtMoney(round2(o.totalCost))}</div>
        </div>
        <div class="muted">${escapeHtml(o.breakdown||'')}</div>
        <div class="bar"><span style="width:${(o.totalCost/max)*100}%;"></span></div>
        <div class="muted" style="margin-top:6px;">${escapeHtml(tag)}</div>
      </div>`;
  }).join('');

  container.innerHTML = `${bestHtml}<h3 style="margin:8px 0;">Todas las opciones</h3>${listHtml}`;
}
function buildSummaryText(data){
  if(!data || !data.options?.length) return 'Sin resultados.';
  const options=[...data.options].sort((a,b)=>(a.totalCost??0)-(b.totalCost??0));
  const best=options[0]; const lines=[];
  lines.push(`Opción más económica: ${modeLabel(best.mode)} — ${fmtMoney(round2(best.totalCost))}`);
  if(best.breakdown) lines.push(`Desglose: ${best.breakdown}`); lines.push('');
  lines.push('Todas las opciones:');
  for(const o of options) lines.push(`• ${modeLabel(o.mode)}: ${fmtMoney(round2(o.totalCost))}${o.breakdown?` — ${o.breakdown}`:''}`);
  return lines.join('\n');
}

// ===== core =====
async function calcularPrecio(){
  const {ok,bad}=validate(); if(!ok){ const nice=bad.map(id=>document.querySelector(`label[for="${id}"]`)?.textContent||id).join(', '); setStatus(`Corrige: ${nice}`,true); return; }
  const payload=readPayload(); const btn=$('#btn-calcular'); if(btn) btn.disabled=true; setStatus('Calculando…',false,true);
  try{
    const res = await fetch(getEndpoint(), { method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify(payload) });
    const raw = await res.text(); if(!res.ok) throw new Error(`HTTP ${res.status} -> ${raw}`);
    const data = JSON.parse(raw);
    clearStatus(); renderResult(data); saveForm();
    window.__lastData=data; window.__lastPayload=payload;
  }catch(err){ console.error(err); setStatus(`Error al calcular: ${err.message}`, true); $('#result').innerHTML=''; }
  finally{ if(btn) btn.disabled=false; }
}

// ===== historial =====
const MAX_HISTORY=10;
function historyAll(){ return ls.get('rc_history',[]); }
function historySaveCurrent(){
  if(!window.__lastData||!window.__lastPayload){ setStatus('Primero calcula para guardar en historial.', true); return; }
  const item={ id:Date.now().toString(), ts:new Date().toISOString(), payload:window.__lastPayload, data:window.__lastData, currency:getCurrency(), note:$('#note')?.value?.trim()||'' };
  const list=historyAll(); list.unshift(item); ls.set('rc_history', list.slice(0,MAX_HISTORY)); setStatus('Guardado en historial.'); historyRender();
}
function historyRemove(id){ ls.set('rc_history', historyAll().filter(x=>x.id!==id)); historyRender(); }
function historyClear(){ ls.del('rc_history'); historyRender(); }
function historyRender(){
  const list=historyAll(); const container=$('#historyList'); const empty=$('#historyEmpty'); if(!container||!empty) return;
  if(list.length===0){ container.innerHTML=''; empty.style.display='block'; return; }
  empty.style.display='none';
  container.innerHTML=list.map(item=>{
    const best=(item.data?.options||[])[0]||item.data?.best; const when=new Date(item.ts).toLocaleString();
    const price=best?fmtMoney(round2(best.totalCost)):'—'; const mode=best?modeLabel(best.mode):'—';
    const noteHtml=item.note?`<div class="tiny">Nota: ${escapeHtml(item.note)}</div>`:'';
    return `
      <div class="history-item" data-id="${item.id}">
        <div class="history-meta">
          <div><strong>${escapeHtml(mode)}</strong> — ${escapeHtml(price)}</div>
          <div class="tiny">${escapeHtml(when)} · Moneda: ${escapeHtml(item.currency)}</div>
          ${noteHtml}
        </div>
        <div class="row">
          <button class="btn ghost" data-action="load">Cargar & recalcular</button>
          <button class="btn ghost" data-action="view">Ver guardado</button>
          <button class="btn secondary" data-action="delete">Borrar</button>
        </div>
      </div>`;
  }).join('');
  container.querySelectorAll('.history-item').forEach(row=>{
    const id=row.getAttribute('data-id');
    row.querySelector('[data-action="delete"]')?.addEventListener('click',()=>historyRemove(id));
    row.querySelector('[data-action="load"]')?.addEventListener('click',()=>{
      const item=historyAll().find(x=>x.id===id); if(!item) return;
      for(const k of Object.keys(item.payload||{})){ const el=$(`#${k}`); if(el) el.value=item.payload[k]; }
      setCurrency(item.currency||'DOP'); if($('#note')) $('#note').value=item.note||''; saveForm(); calcularPrecio();
    });
    row.querySelector('[data-action="view"]')?.addEventListener('click',()=>{
      const item=historyAll().find(x=>x.id===id); if(!item) return;
      window.__lastData=item.data; setCurrency(item.currency||'DOP'); renderResult(item.data); setStatus('Mostrando resultado guardado (snapshot).');
    });
  });
}

// ===== compartir / utilidades =====
function buildShareObject(){ return { payload:readPayload(), currency:getCurrency(), note:$('#note')?.value?.trim()||'' }; }
function shareLinkCopy(){
  const q=btoa(unescape(encodeURIComponent(JSON.stringify(buildShareObject()))));
  const url=`${location.origin}${location.pathname}?q=${q}`;
  navigator.clipboard.writeText(url).then(()=>{ setStatus('Enlace copiado.'); setTimeout(clearStatus,1600); })
    .catch(()=> setStatus('No se pudo copiar el enlace.', true));
}
function tryLoadFromUrl(){
  const q=new URLSearchParams(location.search).get('q'); if(!q) return false;
  try{ const obj=JSON.parse(decodeURIComponent(escape(atob(q))));
    if(obj.payload) for(const [k,v] of Object.entries(obj.payload)){ const el=$(`#${k}`); if(el!=null) el.value=v; }
    if(obj.currency) setCurrency(obj.currency); if(obj.note && $('#note')) $('#note').value=obj.note; saveForm(); calcularPrecio(); setStatus('Datos cargados desde enlace.'); return true;
  }catch{ setStatus('No se pudo leer el enlace.', true); return false; }
}
function copySummary(){
  const text=buildSummaryText(window.__lastData); navigator.clipboard.writeText(text)
    .then(()=>{ setStatus('Resumen copiado.'); setTimeout(clearStatus,1600); })
    .catch(()=> setStatus('No se pudo copiar.', true));
}
function exportJson(){
  const blob=new Blob([JSON.stringify(window.__lastData??{},null,2)],{type:'application/json'});
  const url=URL.createObjectURL(blob); const a=document.createElement('a'); a.href=url; a.download='costos.json'; a.click(); URL.revokeObjectURL(url);
}
function resetForm(){ document.getElementById('form-costos').reset(); $('#result').innerHTML=''; clearStatus(); ls.del('rc_formValues'); ls.del('rc_note'); }
function printPdf(){ window.print(); }

// ===== auto-calc =====
function debounce(fn,ms=400){ let t; return (...args)=>{ clearTimeout(t); t=setTimeout(()=>fn(...args),ms); }; }
const onInputChange=debounce(()=>{ saveForm(); if($('#autoCalc')?.checked) calcularPrecio(); },400);

// ===== init =====
function init(){
  loadForm(); historyRender(); tryLoadFromUrl();
  for(const id of inputs){ const el=$(`#${id}`); if(el) el.addEventListener('input', onInputChange); }
  $('#apiBase')?.addEventListener('input', onInputChange);
  $('#note')?.addEventListener('input', onInputChange);
  $('#autoCalc')?.addEventListener('change', onInputChange);
  $('#currency')?.addEventListener('change', ()=>{ saveForm(); if(window.__lastData) renderResult(window.__lastData); });
  $('#darkMode')?.addEventListener('change',(e)=> applyDark(e.target.checked));

  $('#form-costos')?.addEventListener('submit',(e)=>{ e.preventDefault(); calcularPrecio(); });
  $('#btn-calcular')?.addEventListener('click',(e)=>{ e.preventDefault(); calcularPrecio(); });
  $('#btn-save-history')?.addEventListener('click', historySaveCurrent);
  $('#btn-clear-history')?.addEventListener('click', historyClear);
  $('#btn-copy')?.addEventListener('click', copySummary);
  $('#btn-json')?.addEventListener('click', exportJson);
  $('#btn-reset')?.addEventListener('click', resetForm);
  $('#btn-share')?.addEventListener('click', shareLinkCopy);
  $('#btn-print')?.addEventListener('click', printPdf);

  if($('#autoCalc')?.checked) calcularPrecio();
}
document.readyState==='loading' ? document.addEventListener('DOMContentLoaded', init) : init();
