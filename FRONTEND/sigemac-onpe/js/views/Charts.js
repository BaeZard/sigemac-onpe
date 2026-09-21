/* Gráficos SVG sin librerías (reemplazan a Recharts del prototipo) */
const RED = '#C8102E', NAVY = '#1A2B4A', GOLD = '#D4A017', GREEN = '#2D7D46', INDIGO = '#6366F1';

class Charts {
  static legend(l) { return `<div class="legend">${l.map(([n, c]) => `<span><i style="background:${c}"></i>${n}</span>`).join('')}</div>`; }

  // Barras agrupadas, escala 0–100 %. data: [{n, k1, k2…}]
  static bars(data, keys, colors) {
    const W = 560, H = 220, l = 36, t = 8, b = 24, ih = H - t - b, gw = (W - l) / data.length, bw = Math.min(30, gw / (keys.length + 1));
    let s = `<svg viewBox="0 0 ${W} ${H}" class="chart">`;
    [0, 25, 50, 75, 100].forEach(v => {
      const y = t + ih - ih * v / 100;
      s += `<line x1="${l}" x2="${W}" y1="${y}" y2="${y}" stroke="#eef1f6"/><text x="${l - 6}" y="${y + 3}" text-anchor="end" font-size="10" fill="#64748b">${v}%</text>`;
    });
    data.forEach((d, i) => {
      const x0 = l + i * gw + (gw - bw * keys.length) / 2;
      keys.forEach((k, j) => {
        const h = ih * d[k] / 100;
        s += `<rect x="${x0 + j * bw}" y="${t + ih - h}" width="${bw - 2}" height="${h}" rx="3" fill="${colors[j]}"><title>${esc(d.n)}: ${d[k]}%</title></rect>`;
      });
      s += `<text x="${l + i * gw + gw / 2}" y="${H - 6}" text-anchor="middle" font-size="10" fill="#64748b">${esc(d.n)}</text>`;
    });
    return s + '</svg>';
  }

  // Línea, dominio 60–100 %. data: [{n, v}]
  static line(data) {
    const W = 560, H = 180, l = 36, t = 8, b = 22, ih = H - t - b, min = 60, max = 100;
    const x = i => l + 10 + (W - l - 30) * i / Math.max(1, data.length - 1), y = v => t + ih - ih * (v - min) / (max - min);
    const grid = [60, 70, 80, 90, 100].map(v => `<line x1="${l}" x2="${W}" y1="${y(v)}" y2="${y(v)}" stroke="#eef1f6"/><text x="${l - 6}" y="${y(v) + 3}" text-anchor="end" font-size="10" fill="#64748b">${v}%</text>`).join('');
    return `<svg viewBox="0 0 ${W} ${H}" class="chart">${grid}<polyline points="${data.map((d, i) => `${x(i)},${y(d.v)}`).join(' ')}" fill="none" stroke="${RED}" stroke-width="2.5"/>` +
      data.map((d, i) => `<circle cx="${x(i)}" cy="${y(d.v)}" r="4" fill="${RED}"><title>${esc(d.n)}: ${d.v}%</title></circle><text x="${x(i)}" y="${H - 6}" text-anchor="middle" font-size="10" fill="#64748b">${esc(d.n)}</text>`).join('') + '</svg>';
  }

  // Dona. list: [[nombre, valor, color]]
  static donut(list) {
    const tot = list.reduce((a, x) => a + x[1], 0), r = 60, C = 2 * Math.PI * r;
    let acc = 0;
    const arcs = list.map(([n, v, c]) => {
      const len = C * v / tot;
      const o = `<circle cx="100" cy="100" r="${r}" fill="none" stroke="${c}" stroke-width="22" stroke-dasharray="${len - 2} ${C - len + 2}" stroke-dashoffset="${-acc}"><title>${esc(n)}: ${v}</title></circle>`;
      acc += len; return o;
    }).join('');
    return `<svg viewBox="0 0 200 200" class="chart" style="max-width:220px;display:block;margin:0 auto"><g transform="rotate(-90 100 100)">${arcs}</g></svg>` + this.legend(list.map(([n, , c]) => [n, c]));
  }
}
