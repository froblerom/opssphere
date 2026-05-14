import{a as Re,b as Fr,c as jr,d as zr,e as Br,f as $r,g as Hr,h as xe,i as Mt,j as qr,k as Vr}from"./chunk-AG52A3HW.js";import{$ as ln,$a as Tr,A as tt,B as an,Ca as un,D as sr,Da as It,E as cr,Ea as vr,F as V,Fa as yr,G as xt,Ga as _r,H as j,Ha as wr,I as _,Ia as mn,J as lr,Ja as M,K as b,Ka as St,L as Ct,La as nt,M as dr,Ma as pn,N as v,Na as Rr,O as sn,Oa as xr,P as d,Pa as Cr,Q as ge,Qa as Ir,Ra as Sr,S as lt,Sa as Bt,T as O,Ta as $t,U as dt,Ua as Ht,V as G,Va as Er,W as cn,Wa as Mr,X as ur,Xa as ye,Y as ut,Ya as qt,Z as k,Za as rt,_a as Ar,a as m,aa as mr,ab as Dr,b as E,ba as be,bb as mt,ca as pr,cb as _e,d as Xn,db as we,e as Jn,ea as hr,eb as Nr,f as tr,fa as dn,fb as Or,g as Q,ga as et,gb as kr,h as I,hb as pt,i as D,j as N,jb as Pr,k as f,ka as fr,kb as Lr,l as er,la as ve,m as nn,ma as gr,mb as Et,n as nr,nb as Ur,o as C,p as rr,q as J,r as or,s as fe,u as wt,v as rn,w as on,x as Rt,y as ir,ya as br,z as ar}from"./chunk-3PG4OJ6O.js";var h="primary",oe=Symbol("RouteTitle"),vn=class{params;constructor(r){this.params=r||{}}has(r){return Object.prototype.hasOwnProperty.call(this.params,r)}get(r){if(this.has(r)){let t=this.params[r];return Array.isArray(t)?t[0]:t}return null}getAll(r){if(this.has(r)){let t=this.params[r];return Array.isArray(t)?t:[t]}return[]}get keys(){return Object.keys(this.params)}};function ft(e){return new vn(e)}function hn(e,r,t){for(let n=0;n<e.length;n++){let o=e[n],i=r[n];if(o[0]===":")t[o.substring(1)]=i;else if(o!==i.path)return!1}return!0}function Jr(e,r,t){let n=t.path.split("/"),o=n.indexOf("**");if(o===-1){if(n.length>e.length||t.pathMatch==="full"&&(r.hasChildren()||n.length<e.length))return null;let c={},l=e.slice(0,n.length);return hn(n,l,c)?{consumed:l,posParams:c}:null}if(o!==n.lastIndexOf("**"))return null;let i=n.slice(0,o),s=n.slice(o+1);if(i.length+s.length>e.length||t.pathMatch==="full"&&r.hasChildren()&&t.path!=="**")return null;let a={};return!hn(i,e.slice(0,i.length),a)||!hn(s,e.slice(e.length-s.length),a)?null:{consumed:e,posParams:a}}function Ae(e){return new Promise((r,t)=>{e.pipe(tt()).subscribe({next:n=>r(n),error:n=>t(n)})})}function wi(e,r){if(e.length!==r.length)return!1;for(let t=0;t<e.length;++t)if(!W(e[t],r[t]))return!1;return!0}function W(e,r){let t=e?yn(e):void 0,n=r?yn(r):void 0;if(!t||!n||t.length!=n.length)return!1;let o;for(let i=0;i<t.length;i++)if(o=t[i],!to(e[o],r[o]))return!1;return!0}function yn(e){return[...Object.keys(e),...Object.getOwnPropertySymbols(e)]}function to(e,r){if(Array.isArray(e)&&Array.isArray(r)){if(e.length!==r.length)return!1;let t=[...e].sort(),n=[...r].sort();return t.every((o,i)=>n[i]===o)}else return e===r}function Ri(e){return e.length>0?e[e.length-1]:null}function vt(e){return nn(e)?e:xr(e)?N(Promise.resolve(e)):f(e)}function eo(e){return nn(e)?Ae(e):Promise.resolve(e)}var xi={exact:oo,subset:io},no={exact:Ci,subset:Ii,ignored:()=>!0},ro={paths:"exact",fragment:"ignored",matrixParams:"ignored",queryParams:"exact"},_n={paths:"subset",fragment:"ignored",matrixParams:"ignored",queryParams:"subset"};function Gr(e,r,t){return xi[t.paths](e.root,r.root,t.matrixParams)&&no[t.queryParams](e.queryParams,r.queryParams)&&!(t.fragment==="exact"&&e.fragment!==r.fragment)}function Ci(e,r){return W(e,r)}function oo(e,r,t){if(!ht(e.segments,r.segments)||!Se(e.segments,r.segments,t)||e.numberOfChildren!==r.numberOfChildren)return!1;for(let n in r.children)if(!e.children[n]||!oo(e.children[n],r.children[n],t))return!1;return!0}function Ii(e,r){return Object.keys(r).length<=Object.keys(e).length&&Object.keys(r).every(t=>to(e[t],r[t]))}function io(e,r,t){return ao(e,r,r.segments,t)}function ao(e,r,t,n){if(e.segments.length>t.length){let o=e.segments.slice(0,t.length);return!(!ht(o,t)||r.hasChildren()||!Se(o,t,n))}else if(e.segments.length===t.length){if(!ht(e.segments,t)||!Se(e.segments,t,n))return!1;for(let o in r.children)if(!e.children[o]||!io(e.children[o],r.children[o],n))return!1;return!0}else{let o=t.slice(0,e.segments.length),i=t.slice(e.segments.length);return!ht(e.segments,o)||!Se(e.segments,o,n)||!e.children[h]?!1:ao(e.children[h],r,i,n)}}function Se(e,r,t){return r.every((n,o)=>no[t](e[o].parameters,n.parameters))}var L=class{root;queryParams;fragment;_queryParamMap;constructor(r=new g([],{}),t={},n=null){this.root=r,this.queryParams=t,this.fragment=n}get queryParamMap(){return this._queryParamMap??=ft(this.queryParams),this._queryParamMap}toString(){return Mi.serialize(this)}},g=class{segments;children;parent=null;constructor(r,t){this.segments=r,this.children=t,Object.values(t).forEach(n=>n.parent=this)}hasChildren(){return this.numberOfChildren>0}get numberOfChildren(){return Object.keys(this.children).length}toString(){return Ee(this)}},ot=class{path;parameters;_parameterMap;constructor(r,t){this.path=r,this.parameters=t}get parameterMap(){return this._parameterMap??=ft(this.parameters),this._parameterMap}toString(){return co(this)}};function Si(e,r){return ht(e,r)&&e.every((t,n)=>W(t.parameters,r[n].parameters))}function ht(e,r){return e.length!==r.length?!1:e.every((t,n)=>t.path===r[n].path)}function Ei(e,r){let t=[];return Object.entries(e.children).forEach(([n,o])=>{n===h&&(t=t.concat(r(o,n)))}),Object.entries(e.children).forEach(([n,o])=>{n!==h&&(t=t.concat(r(o,n)))}),t}var ie=(()=>{class e{static \u0275fac=function(n){return new(n||e)};static \u0275prov=b({token:e,factory:()=>new it,providedIn:"root"})}return e})(),it=class{parse(r){let t=new Rn(r);return new L(t.parseRootSegment(),t.parseQueryParams(),t.parseFragment())}serialize(r){let t=`/${Vt(r.root,!0)}`,n=Di(r.queryParams),o=typeof r.fragment=="string"?`#${Ai(r.fragment)}`:"";return`${t}${n}${o}`}},Mi=new it;function Ee(e){return e.segments.map(r=>co(r)).join("/")}function Vt(e,r){if(!e.hasChildren())return Ee(e);if(r){let t=e.children[h]?Vt(e.children[h],!1):"",n=[];return Object.entries(e.children).forEach(([o,i])=>{o!==h&&n.push(`${o}:${Vt(i,!1)}`)}),n.length>0?`${t}(${n.join("//")})`:t}else{let t=Ei(e,(n,o)=>o===h?[Vt(e.children[h],!1)]:[`${o}:${Vt(n,!1)}`]);return Object.keys(e.children).length===1&&e.children[h]!=null?`${Ee(e)}/${t[0]}`:`${Ee(e)}/(${t.join("//")})`}}function so(e){return encodeURIComponent(e).replace(/%40/g,"@").replace(/%3A/gi,":").replace(/%24/g,"$").replace(/%2C/gi,",")}function Ce(e){return so(e).replace(/%3B/gi,";")}function Ai(e){return encodeURI(e)}function wn(e){return so(e).replace(/\(/g,"%28").replace(/\)/g,"%29").replace(/%26/gi,"&")}function Me(e){return decodeURIComponent(e)}function Wr(e){return Me(e.replace(/\+/g,"%20"))}function co(e){return`${wn(e.path)}${Ti(e.parameters)}`}function Ti(e){return Object.entries(e).map(([r,t])=>`;${wn(r)}=${wn(t)}`).join("")}function Di(e){let r=Object.entries(e).map(([t,n])=>Array.isArray(n)?n.map(o=>`${Ce(t)}=${Ce(o)}`).join("&"):`${Ce(t)}=${Ce(n)}`).filter(t=>t);return r.length?`?${r.join("&")}`:""}var Ni=/^[^\/()?;#]+/;function fn(e){let r=e.match(Ni);return r?r[0]:""}var Oi=/^[^\/()?;=#]+/;function ki(e){let r=e.match(Oi);return r?r[0]:""}var Pi=/^[^=?&#]+/;function Li(e){let r=e.match(Pi);return r?r[0]:""}var Ui=/^[^&#]+/;function Fi(e){let r=e.match(Ui);return r?r[0]:""}var Rn=class{url;remaining;constructor(r){this.url=r,this.remaining=r}parseRootSegment(){for(;this.consumeOptional("/"););return this.remaining===""||this.peekStartsWith("?")||this.peekStartsWith("#")?new g([],{}):new g([],this.parseChildren())}parseQueryParams(){let r={};if(this.consumeOptional("?"))do this.parseQueryParam(r);while(this.consumeOptional("&"));return r}parseFragment(){return this.consumeOptional("#")?decodeURIComponent(this.remaining):null}parseChildren(r=0){if(r>50)throw new _(4010,!1);if(this.remaining==="")return{};this.consumeOptional("/");let t=[];for(this.peekStartsWith("(")||t.push(this.parseSegment());this.peekStartsWith("/")&&!this.peekStartsWith("//")&&!this.peekStartsWith("/(");)this.capture("/"),t.push(this.parseSegment());let n={};this.peekStartsWith("/(")&&(this.capture("/"),n=this.parseParens(!0,r));let o={};return this.peekStartsWith("(")&&(o=this.parseParens(!1,r)),(t.length>0||Object.keys(n).length>0)&&(o[h]=new g(t,n)),o}parseSegment(){let r=fn(this.remaining);if(r===""&&this.peekStartsWith(";"))throw new _(4009,!1);return this.capture(r),new ot(Me(r),this.parseMatrixParams())}parseMatrixParams(){let r={};for(;this.consumeOptional(";");)this.parseParam(r);return r}parseParam(r){let t=ki(this.remaining);if(!t)return;this.capture(t);let n="";if(this.consumeOptional("=")){let o=fn(this.remaining);o&&(n=o,this.capture(n))}r[Me(t)]=Me(n)}parseQueryParam(r){let t=Li(this.remaining);if(!t)return;this.capture(t);let n="";if(this.consumeOptional("=")){let s=Fi(this.remaining);s&&(n=s,this.capture(n))}let o=Wr(t),i=Wr(n);if(r.hasOwnProperty(o)){let s=r[o];Array.isArray(s)||(s=[s],r[o]=s),s.push(i)}else r[o]=i}parseParens(r,t){let n={};for(this.capture("(");!this.consumeOptional(")")&&this.remaining.length>0;){let o=fn(this.remaining),i=this.remaining[o.length];if(i!=="/"&&i!==")"&&i!==";")throw new _(4010,!1);let s;o.indexOf(":")>-1?(s=o.slice(0,o.indexOf(":")),this.capture(s),this.capture(":")):r&&(s=h);let a=this.parseChildren(t+1);n[s??h]=Object.keys(a).length===1&&a[h]?a[h]:new g([],a),this.consumeOptional("//")}return n}peekStartsWith(r){return this.remaining.startsWith(r)}consumeOptional(r){return this.peekStartsWith(r)?(this.remaining=this.remaining.substring(r.length),!0):!1}capture(r){if(!this.consumeOptional(r))throw new _(4011,!1)}};function lo(e){return e.segments.length>0?new g([],{[h]:e}):e}function uo(e){let r={};for(let[n,o]of Object.entries(e.children)){let i=uo(o);if(n===h&&i.segments.length===0&&i.hasChildren())for(let[s,a]of Object.entries(i.children))r[s]=a;else(i.segments.length>0||i.hasChildren())&&(r[n]=i)}let t=new g(e.segments,r);return ji(t)}function ji(e){if(e.numberOfChildren===1&&e.children[h]){let r=e.children[h];return new g(e.segments.concat(r.segments),r.children)}return e}function Nt(e){return e instanceof L}function mo(e,r,t=null,n=null,o=new it){let i=po(e);return ho(i,r,t,n,o)}function po(e){let r;function t(i){let s={};for(let c of i.children){let l=t(c);s[c.outlet]=l}let a=new g(i.url,s);return i===e&&(r=a),a}let n=t(e.root),o=lo(n);return r??o}function ho(e,r,t,n,o){let i=e;for(;i.parent;)i=i.parent;if(r.length===0)return gn(i,i,i,t,n,o);let s=zi(r);if(s.toRoot())return gn(i,i,new g([],{}),t,n,o);let a=Bi(s,i,e),c=a.processChildren?Wt(a.segmentGroup,a.index,s.commands):go(a.segmentGroup,a.index,s.commands);return gn(i,a.segmentGroup,c,t,n,o)}function Te(e){return typeof e=="object"&&e!=null&&!e.outlets&&!e.segmentPath}function Kt(e){return typeof e=="object"&&e!=null&&e.outlets}function Zr(e,r,t){e||="\u0275";let n=new L;return n.queryParams={[e]:r},t.parse(t.serialize(n)).queryParams[e]}function gn(e,r,t,n,o,i){let s={};for(let[l,u]of Object.entries(n??{}))s[l]=Array.isArray(u)?u.map(p=>Zr(l,p,i)):Zr(l,u,i);let a;e===r?a=t:a=fo(e,r,t);let c=lo(uo(a));return new L(c,s,o)}function fo(e,r,t){let n={};return Object.entries(e.children).forEach(([o,i])=>{i===r?n[o]=t:n[o]=fo(i,r,t)}),new g(e.segments,n)}var De=class{isAbsolute;numberOfDoubleDots;commands;constructor(r,t,n){if(this.isAbsolute=r,this.numberOfDoubleDots=t,this.commands=n,r&&n.length>0&&Te(n[0]))throw new _(4003,!1);let o=n.find(Kt);if(o&&o!==Ri(n))throw new _(4004,!1)}toRoot(){return this.isAbsolute&&this.commands.length===1&&this.commands[0]=="/"}};function zi(e){if(typeof e[0]=="string"&&e.length===1&&e[0]==="/")return new De(!0,0,e);let r=0,t=!1,n=e.reduce((o,i,s)=>{if(typeof i=="object"&&i!=null){if(i.outlets){let a={};return Object.entries(i.outlets).forEach(([c,l])=>{a[c]=typeof l=="string"?l.split("/"):l}),[...o,{outlets:a}]}if(i.segmentPath)return[...o,i.segmentPath]}return typeof i!="string"?[...o,i]:s===0?(i.split("/").forEach((a,c)=>{c==0&&a==="."||(c==0&&a===""?t=!0:a===".."?r++:a!=""&&o.push(a))}),o):[...o,i]},[]);return new De(t,r,n)}var Tt=class{segmentGroup;processChildren;index;constructor(r,t,n){this.segmentGroup=r,this.processChildren=t,this.index=n}};function Bi(e,r,t){if(e.isAbsolute)return new Tt(r,!0,0);if(!t)return new Tt(r,!1,NaN);if(t.parent===null)return new Tt(t,!0,0);let n=Te(e.commands[0])?0:1,o=t.segments.length-1+n;return $i(t,o,e.numberOfDoubleDots)}function $i(e,r,t){let n=e,o=r,i=t;for(;i>o;){if(i-=o,n=n.parent,!n)throw new _(4005,!1);o=n.segments.length}return new Tt(n,!1,o-i)}function Hi(e){return Kt(e[0])?e[0].outlets:{[h]:e}}function go(e,r,t){if(e??=new g([],{}),e.segments.length===0&&e.hasChildren())return Wt(e,r,t);let n=qi(e,r,t),o=t.slice(n.commandIndex);if(n.match&&n.pathIndex<e.segments.length){let i=new g(e.segments.slice(0,n.pathIndex),{});return i.children[h]=new g(e.segments.slice(n.pathIndex),e.children),Wt(i,0,o)}else return n.match&&o.length===0?new g(e.segments,{}):n.match&&!e.hasChildren()?xn(e,r,t):n.match?Wt(e,0,o):xn(e,r,t)}function Wt(e,r,t){if(t.length===0)return new g(e.segments,{});{let n=Hi(t),o={};if(Object.keys(n).some(i=>i!==h)&&e.children[h]&&e.numberOfChildren===1&&e.children[h].segments.length===0){let i=Wt(e.children[h],r,t);return new g(e.segments,i.children)}return Object.entries(n).forEach(([i,s])=>{typeof s=="string"&&(s=[s]),s!==null&&(o[i]=go(e.children[i],r,s))}),Object.entries(e.children).forEach(([i,s])=>{n[i]===void 0&&(o[i]=s)}),new g(e.segments,o)}}function qi(e,r,t){let n=0,o=r,i={match:!1,pathIndex:0,commandIndex:0};for(;o<e.segments.length;){if(n>=t.length)return i;let s=e.segments[o],a=t[n];if(Kt(a))break;let c=`${a}`,l=n<t.length-1?t[n+1]:null;if(o>0&&c===void 0)break;if(c&&l&&typeof l=="object"&&l.outlets===void 0){if(!Kr(c,l,s))return i;n+=2}else{if(!Kr(c,{},s))return i;n++}o++}return{match:!0,pathIndex:o,commandIndex:n}}function xn(e,r,t){let n=e.segments.slice(0,r),o=0;for(;o<t.length;){let i=t[o];if(Kt(i)){let c=Vi(i.outlets);return new g(n,c)}if(o===0&&Te(t[0])){let c=e.segments[r];n.push(new ot(c.path,Qr(t[0]))),o++;continue}let s=Kt(i)?i.outlets[h]:`${i}`,a=o<t.length-1?t[o+1]:null;s&&a&&Te(a)?(n.push(new ot(s,Qr(a))),o+=2):(n.push(new ot(s,{})),o++)}return new g(n,{})}function Vi(e){let r={};return Object.entries(e).forEach(([t,n])=>{typeof n=="string"&&(n=[n]),n!==null&&(r[t]=xn(new g([],{}),0,n))}),r}function Qr(e){let r={};return Object.entries(e).forEach(([t,n])=>r[t]=`${n}`),r}function Kr(e,r,t){return e==t.path&&W(r,t.parameters)}var Zt="imperative",w=(function(e){return e[e.NavigationStart=0]="NavigationStart",e[e.NavigationEnd=1]="NavigationEnd",e[e.NavigationCancel=2]="NavigationCancel",e[e.NavigationError=3]="NavigationError",e[e.RoutesRecognized=4]="RoutesRecognized",e[e.ResolveStart=5]="ResolveStart",e[e.ResolveEnd=6]="ResolveEnd",e[e.GuardsCheckStart=7]="GuardsCheckStart",e[e.GuardsCheckEnd=8]="GuardsCheckEnd",e[e.RouteConfigLoadStart=9]="RouteConfigLoadStart",e[e.RouteConfigLoadEnd=10]="RouteConfigLoadEnd",e[e.ChildActivationStart=11]="ChildActivationStart",e[e.ChildActivationEnd=12]="ChildActivationEnd",e[e.ActivationStart=13]="ActivationStart",e[e.ActivationEnd=14]="ActivationEnd",e[e.Scroll=15]="Scroll",e[e.NavigationSkipped=16]="NavigationSkipped",e})(w||{}),T=class{id;url;constructor(r,t){this.id=r,this.url=t}},gt=class extends T{type=w.NavigationStart;navigationTrigger;restoredState;constructor(r,t,n="imperative",o=null){super(r,t),this.navigationTrigger=n,this.restoredState=o}toString(){return`NavigationStart(id: ${this.id}, url: '${this.url}')`}},Y=class extends T{urlAfterRedirects;type=w.NavigationEnd;constructor(r,t,n){super(r,t),this.urlAfterRedirects=n}toString(){return`NavigationEnd(id: ${this.id}, url: '${this.url}', urlAfterRedirects: '${this.urlAfterRedirects}')`}},R=(function(e){return e[e.Redirect=0]="Redirect",e[e.SupersededByNewNavigation=1]="SupersededByNewNavigation",e[e.NoDataFromResolver=2]="NoDataFromResolver",e[e.GuardRejected=3]="GuardRejected",e[e.Aborted=4]="Aborted",e})(R||{}),Yt=(function(e){return e[e.IgnoredSameUrlNavigation=0]="IgnoredSameUrlNavigation",e[e.IgnoredByUrlHandlingStrategy=1]="IgnoredByUrlHandlingStrategy",e})(Yt||{}),P=class extends T{reason;code;type=w.NavigationCancel;constructor(r,t,n,o){super(r,t),this.reason=n,this.code=o}toString(){return`NavigationCancel(id: ${this.id}, url: '${this.url}')`}};function bo(e){return e instanceof P&&(e.code===R.Redirect||e.code===R.SupersededByNewNavigation)}var X=class extends T{reason;code;type=w.NavigationSkipped;constructor(r,t,n,o){super(r,t),this.reason=n,this.code=o}},bt=class extends T{error;target;type=w.NavigationError;constructor(r,t,n,o){super(r,t),this.error=n,this.target=o}toString(){return`NavigationError(id: ${this.id}, url: '${this.url}', error: ${this.error})`}},Xt=class extends T{urlAfterRedirects;state;type=w.RoutesRecognized;constructor(r,t,n,o){super(r,t),this.urlAfterRedirects=n,this.state=o}toString(){return`RoutesRecognized(id: ${this.id}, url: '${this.url}', urlAfterRedirects: '${this.urlAfterRedirects}', state: ${this.state})`}},Ne=class extends T{urlAfterRedirects;state;type=w.GuardsCheckStart;constructor(r,t,n,o){super(r,t),this.urlAfterRedirects=n,this.state=o}toString(){return`GuardsCheckStart(id: ${this.id}, url: '${this.url}', urlAfterRedirects: '${this.urlAfterRedirects}', state: ${this.state})`}},Oe=class extends T{urlAfterRedirects;state;shouldActivate;type=w.GuardsCheckEnd;constructor(r,t,n,o,i){super(r,t),this.urlAfterRedirects=n,this.state=o,this.shouldActivate=i}toString(){return`GuardsCheckEnd(id: ${this.id}, url: '${this.url}', urlAfterRedirects: '${this.urlAfterRedirects}', state: ${this.state}, shouldActivate: ${this.shouldActivate})`}},ke=class extends T{urlAfterRedirects;state;type=w.ResolveStart;constructor(r,t,n,o){super(r,t),this.urlAfterRedirects=n,this.state=o}toString(){return`ResolveStart(id: ${this.id}, url: '${this.url}', urlAfterRedirects: '${this.urlAfterRedirects}', state: ${this.state})`}},Pe=class extends T{urlAfterRedirects;state;type=w.ResolveEnd;constructor(r,t,n,o){super(r,t),this.urlAfterRedirects=n,this.state=o}toString(){return`ResolveEnd(id: ${this.id}, url: '${this.url}', urlAfterRedirects: '${this.urlAfterRedirects}', state: ${this.state})`}},Le=class{route;type=w.RouteConfigLoadStart;constructor(r){this.route=r}toString(){return`RouteConfigLoadStart(path: ${this.route.path})`}},Ue=class{route;type=w.RouteConfigLoadEnd;constructor(r){this.route=r}toString(){return`RouteConfigLoadEnd(path: ${this.route.path})`}},Fe=class{snapshot;type=w.ChildActivationStart;constructor(r){this.snapshot=r}toString(){return`ChildActivationStart(path: '${this.snapshot.routeConfig&&this.snapshot.routeConfig.path||""}')`}},je=class{snapshot;type=w.ChildActivationEnd;constructor(r){this.snapshot=r}toString(){return`ChildActivationEnd(path: '${this.snapshot.routeConfig&&this.snapshot.routeConfig.path||""}')`}},ze=class{snapshot;type=w.ActivationStart;constructor(r){this.snapshot=r}toString(){return`ActivationStart(path: '${this.snapshot.routeConfig&&this.snapshot.routeConfig.path||""}')`}},Be=class{snapshot;type=w.ActivationEnd;constructor(r){this.snapshot=r}toString(){return`ActivationEnd(path: '${this.snapshot.routeConfig&&this.snapshot.routeConfig.path||""}')`}};var Ot=class{},Jt=class{},kt=class{url;navigationBehaviorOptions;constructor(r,t){this.url=r,this.navigationBehaviorOptions=t}};function Gi(e){return!(e instanceof Ot)&&!(e instanceof kt)&&!(e instanceof Jt)}var $e=class{rootInjector;outlet=null;route=null;children;attachRef=null;get injector(){return this.route?.snapshot._environmentInjector??this.rootInjector}constructor(r){this.rootInjector=r,this.children=new Ft(this.rootInjector)}},Ft=(()=>{class e{rootInjector;contexts=new Map;constructor(t){this.rootInjector=t}onChildOutletCreated(t,n){let o=this.getOrCreateContext(t);o.outlet=n,this.contexts.set(t,o)}onChildOutletDestroyed(t){let n=this.getContext(t);n&&(n.outlet=null,n.attachRef=null)}onOutletDeactivated(){let t=this.contexts;return this.contexts=new Map,t}onOutletReAttached(t){this.contexts=t}getOrCreateContext(t){let n=this.getContext(t);return n||(n=new $e(this.rootInjector),this.contexts.set(t,n)),n}getContext(t){return this.contexts.get(t)||null}static \u0275fac=function(n){return new(n||e)(sn(lt))};static \u0275prov=b({token:e,factory:e.\u0275fac,providedIn:"root"})}return e})(),He=class{_root;constructor(r){this._root=r}get root(){return this._root.value}parent(r){let t=this.pathFromRoot(r);return t.length>1?t[t.length-2]:null}children(r){let t=Cn(r,this._root);return t?t.children.map(n=>n.value):[]}firstChild(r){let t=Cn(r,this._root);return t&&t.children.length>0?t.children[0].value:null}siblings(r){let t=In(r,this._root);return t.length<2?[]:t[t.length-2].children.map(o=>o.value).filter(o=>o!==r)}pathFromRoot(r){return In(r,this._root).map(t=>t.value)}};function Cn(e,r){if(e===r.value)return r;for(let t of r.children){let n=Cn(e,t);if(n)return n}return null}function In(e,r){if(e===r.value)return[r];for(let t of r.children){let n=In(e,t);if(n.length)return n.unshift(r),n}return[]}var A=class{value;children;constructor(r,t){this.value=r,this.children=t}toString(){return`TreeNode(${this.value})`}};function At(e){let r={};return e&&e.children.forEach(t=>r[t.value.outlet]=t),r}var te=class extends He{snapshot;constructor(r,t){super(r),this.snapshot=t,kn(this,r)}toString(){return this.snapshot.toString()}};function vo(e,r){let t=Wi(e,r),n=new I([new ot("",{})]),o=new I({}),i=new I({}),s=new I({}),a=new I(""),c=new at(n,o,s,a,i,h,e,t.root);return c.snapshot=t.root,new te(new A(c,[]),t)}function Wi(e,r){let t={},n={},o={},s=new Pt([],t,o,"",n,h,e,null,{},r);return new ee("",new A(s,[]))}var at=class{urlSubject;paramsSubject;queryParamsSubject;fragmentSubject;dataSubject;outlet;component;snapshot;_futureSnapshot;_routerState;_paramMap;_queryParamMap;title;url;params;queryParams;fragment;data;constructor(r,t,n,o,i,s,a,c){this.urlSubject=r,this.paramsSubject=t,this.queryParamsSubject=n,this.fragmentSubject=o,this.dataSubject=i,this.outlet=s,this.component=a,this._futureSnapshot=c,this.title=this.dataSubject?.pipe(C(l=>l[oe]))??f(void 0),this.url=r,this.params=t,this.queryParams=n,this.fragment=o,this.data=i}get routeConfig(){return this._futureSnapshot.routeConfig}get root(){return this._routerState.root}get parent(){return this._routerState.parent(this)}get firstChild(){return this._routerState.firstChild(this)}get children(){return this._routerState.children(this)}get pathFromRoot(){return this._routerState.pathFromRoot(this)}get paramMap(){return this._paramMap??=this.params.pipe(C(r=>ft(r))),this._paramMap}get queryParamMap(){return this._queryParamMap??=this.queryParams.pipe(C(r=>ft(r))),this._queryParamMap}toString(){return this.snapshot?this.snapshot.toString():`Future(${this._futureSnapshot})`}};function On(e,r,t="emptyOnly"){let n,{routeConfig:o}=e;return r!==null&&(t==="always"||o?.path===""||!r.component&&!r.routeConfig?.loadComponent)?n={params:m(m({},r.params),e.params),data:m(m({},r.data),e.data),resolve:m(m(m(m({},e.data),r.data),o?.data),e._resolvedData)}:n={params:m({},e.params),data:m({},e.data),resolve:m(m({},e.data),e._resolvedData??{})},o&&_o(o)&&(n.resolve[oe]=o.title),n}var Pt=class{url;params;queryParams;fragment;data;outlet;component;routeConfig;_resolve;_resolvedData;_routerState;_paramMap;_queryParamMap;_environmentInjector;get title(){return this.data?.[oe]}constructor(r,t,n,o,i,s,a,c,l,u){this.url=r,this.params=t,this.queryParams=n,this.fragment=o,this.data=i,this.outlet=s,this.component=a,this.routeConfig=c,this._resolve=l,this._environmentInjector=u}get root(){return this._routerState.root}get parent(){return this._routerState.parent(this)}get firstChild(){return this._routerState.firstChild(this)}get children(){return this._routerState.children(this)}get pathFromRoot(){return this._routerState.pathFromRoot(this)}get paramMap(){return this._paramMap??=ft(this.params),this._paramMap}get queryParamMap(){return this._queryParamMap??=ft(this.queryParams),this._queryParamMap}toString(){let r=this.url.map(n=>n.toString()).join("/"),t=this.routeConfig?this.routeConfig.path:"";return`Route(url:'${r}', path:'${t}')`}},ee=class extends He{url;constructor(r,t){super(t),this.url=r,kn(this,t)}toString(){return yo(this._root)}};function kn(e,r){r.value._routerState=e,r.children.forEach(t=>kn(e,t))}function yo(e){let r=e.children.length>0?` { ${e.children.map(yo).join(", ")} } `:"";return`${e.value}${r}`}function bn(e){if(e.snapshot){let r=e.snapshot,t=e._futureSnapshot;e.snapshot=t,W(r.queryParams,t.queryParams)||e.queryParamsSubject.next(t.queryParams),r.fragment!==t.fragment&&e.fragmentSubject.next(t.fragment),W(r.params,t.params)||e.paramsSubject.next(t.params),wi(r.url,t.url)||e.urlSubject.next(t.url),W(r.data,t.data)||e.dataSubject.next(t.data)}else e.snapshot=e._futureSnapshot,e.dataSubject.next(e._futureSnapshot.data)}function Sn(e,r){let t=W(e.params,r.params)&&Si(e.url,r.url),n=!e.parent!=!r.parent;return t&&!n&&(!e.parent||Sn(e.parent,r.parent))}function _o(e){return typeof e.title=="string"||e.title===null}var wo=new v(""),ae=(()=>{class e{activated=null;get activatedComponentRef(){return this.activated}_activatedRoute=null;name=h;activateEvents=new ut;deactivateEvents=new ut;attachEvents=new ut;detachEvents=new ut;routerOutletData=Pr();parentContexts=d(Ft);location=d(_r);changeDetector=d(Lr);inputBinder=d(We,{optional:!0});supportsBindingToComponentInputs=!0;ngOnChanges(t){if(t.name){let{firstChange:n,previousValue:o}=t.name;if(n)return;this.isTrackedInParentContexts(o)&&(this.deactivate(),this.parentContexts.onChildOutletDestroyed(o)),this.initializeOutletWithName()}}ngOnDestroy(){this.isTrackedInParentContexts(this.name)&&this.parentContexts.onChildOutletDestroyed(this.name),this.inputBinder?.unsubscribeFromRouteData(this)}isTrackedInParentContexts(t){return this.parentContexts.getContext(t)?.outlet===this}ngOnInit(){this.initializeOutletWithName()}initializeOutletWithName(){if(this.parentContexts.onChildOutletCreated(this.name,this),this.activated)return;let t=this.parentContexts.getContext(this.name);t?.route&&(t.attachRef?this.attach(t.attachRef,t.route):this.activateWith(t.route,t.injector))}get isActivated(){return!!this.activated}get component(){if(!this.activated)throw new _(4012,!1);return this.activated.instance}get activatedRoute(){if(!this.activated)throw new _(4012,!1);return this._activatedRoute}get activatedRouteData(){return this._activatedRoute?this._activatedRoute.snapshot.data:{}}detach(){if(!this.activated)throw new _(4012,!1);this.location.detach();let t=this.activated;return this.activated=null,this._activatedRoute=null,this.detachEvents.emit(t.instance),t}attach(t,n){this.activated=t,this._activatedRoute=n,this.location.insert(t.hostView),this.inputBinder?.bindActivatedRouteToOutletComponent(this),this.attachEvents.emit(t.instance)}deactivate(){if(this.activated){let t=this.component;this.activated.destroy(),this.activated=null,this._activatedRoute=null,this.deactivateEvents.emit(t)}}activateWith(t,n){if(this.isActivated)throw new _(4013,!1);this._activatedRoute=t;let o=this.location,s=t.snapshot.component,a=this.parentContexts.getOrCreateContext(this.name).children,c=new En(t,a,o.injector,this.routerOutletData);this.activated=o.createComponent(s,{index:o.length,injector:c,environmentInjector:n}),this.changeDetector.markForCheck(),this.inputBinder?.bindActivatedRouteToOutletComponent(this),this.activateEvents.emit(this.activated.instance)}static \u0275fac=function(n){return new(n||e)};static \u0275dir=nt({type:e,selectors:[["router-outlet"]],inputs:{name:"name",routerOutletData:[1,"routerOutletData"]},outputs:{activateEvents:"activate",deactivateEvents:"deactivate",attachEvents:"attach",detachEvents:"detach"},exportAs:["outlet"],features:[hr]})}return e})(),En=class{route;childContexts;parent;outletData;constructor(r,t,n,o){this.route=r,this.childContexts=t,this.parent=n,this.outletData=o}get(r,t){return r===at?this.route:r===Ft?this.childContexts:r===wo?this.outletData:this.parent.get(r,t)}},We=new v("");var Pn=(()=>{class e{static \u0275fac=function(n){return new(n||e)};static \u0275cmp=M({type:e,selectors:[["ng-component"]],exportAs:["emptyRouterOutlet"],decls:1,vars:0,template:function(n,o){n&1&&Ht(0,"router-outlet")},dependencies:[ae],encapsulation:2})}return e})();function Ln(e){let r=e.children&&e.children.map(Ln),t=r?E(m({},e),{children:r}):m({},e);return!t.component&&!t.loadComponent&&(r||t.loadChildren)&&t.outlet&&t.outlet!==h&&(t.component=Pn),t}function Zi(e,r,t){let n=ne(e,r._root,t?t._root:void 0);return new te(n,r)}function ne(e,r,t){if(t&&e.shouldReuseRoute(r.value,t.value.snapshot)){let n=t.value;n._futureSnapshot=r.value;let o=Qi(e,r,t);return new A(n,o)}else{if(e.shouldAttach(r.value)){let i=e.retrieve(r.value);if(i!==null){let s=i.route;return s.value._futureSnapshot=r.value,s.children=r.children.map(a=>ne(e,a)),s}}let n=Ki(r.value),o=r.children.map(i=>ne(e,i));return new A(n,o)}}function Qi(e,r,t){return r.children.map(n=>{for(let o of t.children)if(e.shouldReuseRoute(n.value,o.value.snapshot))return ne(e,n,o);return ne(e,n)})}function Ki(e){return new at(new I(e.url),new I(e.params),new I(e.queryParams),new I(e.fragment),new I(e.data),e.outlet,e.component,e)}var Lt=class{redirectTo;navigationBehaviorOptions;constructor(r,t){this.redirectTo=r,this.navigationBehaviorOptions=t}},Ro="ngNavigationCancelingError";function qe(e,r){let{redirectTo:t,navigationBehaviorOptions:n}=Nt(r)?{redirectTo:r,navigationBehaviorOptions:void 0}:r,o=xo(!1,R.Redirect);return o.url=t,o.navigationBehaviorOptions=n,o}function xo(e,r){let t=new Error(`NavigationCancelingError: ${e||""}`);return t[Ro]=!0,t.cancellationCode=r,t}function Yi(e){return Co(e)&&Nt(e.url)}function Co(e){return!!e&&e[Ro]}var Mn=class{routeReuseStrategy;futureState;currState;forwardEvent;inputBindingEnabled;constructor(r,t,n,o,i){this.routeReuseStrategy=r,this.futureState=t,this.currState=n,this.forwardEvent=o,this.inputBindingEnabled=i}activate(r){let t=this.futureState._root,n=this.currState?this.currState._root:null;this.deactivateChildRoutes(t,n,r),bn(this.futureState.root),this.activateChildRoutes(t,n,r)}deactivateChildRoutes(r,t,n){let o=At(t);r.children.forEach(i=>{let s=i.value.outlet;this.deactivateRoutes(i,o[s],n),delete o[s]}),Object.values(o).forEach(i=>{this.deactivateRouteAndItsChildren(i,n)})}deactivateRoutes(r,t,n){let o=r.value,i=t?t.value:null;if(o===i)if(o.component){let s=n.getContext(o.outlet);s&&this.deactivateChildRoutes(r,t,s.children)}else this.deactivateChildRoutes(r,t,n);else i&&this.deactivateRouteAndItsChildren(t,n)}deactivateRouteAndItsChildren(r,t){r.value.component&&this.routeReuseStrategy.shouldDetach(r.value.snapshot)?this.detachAndStoreRouteSubtree(r,t):this.deactivateRouteAndOutlet(r,t)}detachAndStoreRouteSubtree(r,t){let n=t.getContext(r.value.outlet),o=n&&r.value.component?n.children:t,i=At(r);for(let s of Object.values(i))this.deactivateRouteAndItsChildren(s,o);if(n&&n.outlet){let s=n.outlet.detach(),a=n.children.onOutletDeactivated();this.routeReuseStrategy.store(r.value.snapshot,{componentRef:s,route:r,contexts:a})}}deactivateRouteAndOutlet(r,t){let n=t.getContext(r.value.outlet),o=n&&r.value.component?n.children:t,i=At(r);for(let s of Object.values(i))this.deactivateRouteAndItsChildren(s,o);n&&(n.outlet&&(n.outlet.deactivate(),n.children.onOutletDeactivated()),n.attachRef=null,n.route=null)}activateChildRoutes(r,t,n){let o=At(t);r.children.forEach(i=>{this.activateRoutes(i,o[i.value.outlet],n),this.forwardEvent(new Be(i.value.snapshot))}),r.children.length&&this.forwardEvent(new je(r.value.snapshot))}activateRoutes(r,t,n){let o=r.value,i=t?t.value:null;if(bn(o),o===i)if(o.component){let s=n.getOrCreateContext(o.outlet);this.activateChildRoutes(r,t,s.children)}else this.activateChildRoutes(r,t,n);else if(o.component){let s=n.getOrCreateContext(o.outlet);if(this.routeReuseStrategy.shouldAttach(o.snapshot)){let a=this.routeReuseStrategy.retrieve(o.snapshot);this.routeReuseStrategy.store(o.snapshot,null),s.children.onOutletReAttached(a.contexts),s.attachRef=a.componentRef,s.route=a.route.value,s.outlet&&s.outlet.attach(a.componentRef,a.route.value),bn(a.route.value),this.activateChildRoutes(r,null,s.children)}else s.attachRef=null,s.route=o,s.outlet&&s.outlet.activateWith(o,s.injector),this.activateChildRoutes(r,null,s.children)}else this.activateChildRoutes(r,null,n)}},Ve=class{path;route;constructor(r){this.path=r,this.route=this.path[this.path.length-1]}},Dt=class{component;route;constructor(r,t){this.component=r,this.route=t}};function Xi(e,r,t){let n=e._root,o=r?r._root:null;return Gt(n,o,t,[n.value])}function Ji(e){let r=e.routeConfig?e.routeConfig.canActivateChild:null;return!r||r.length===0?null:{node:e,guards:r}}function jt(e,r){let t=Symbol(),n=r.get(e,t);return n===t?typeof e=="function"&&!dr(e)?e:r.get(e):n}function Gt(e,r,t,n,o={canDeactivateChecks:[],canActivateChecks:[]}){let i=At(r);return e.children.forEach(s=>{ta(s,i[s.value.outlet],t,n.concat([s.value]),o),delete i[s.value.outlet]}),Object.entries(i).forEach(([s,a])=>Qt(a,t.getContext(s),o)),o}function ta(e,r,t,n,o={canDeactivateChecks:[],canActivateChecks:[]}){let i=e.value,s=r?r.value:null,a=t?t.getContext(e.value.outlet):null;if(s&&i.routeConfig===s.routeConfig){let c=ea(s,i,i.routeConfig.runGuardsAndResolvers);c?o.canActivateChecks.push(new Ve(n)):(i.data=s.data,i._resolvedData=s._resolvedData),i.component?Gt(e,r,a?a.children:null,n,o):Gt(e,r,t,n,o),c&&a&&a.outlet&&a.outlet.isActivated&&o.canDeactivateChecks.push(new Dt(a.outlet.component,s))}else s&&Qt(r,a,o),o.canActivateChecks.push(new Ve(n)),i.component?Gt(e,null,a?a.children:null,n,o):Gt(e,null,t,n,o);return o}function ea(e,r,t){if(typeof t=="function")return O(r._environmentInjector,()=>t(e,r));switch(t){case"pathParamsChange":return!ht(e.url,r.url);case"pathParamsOrQueryParamsChange":return!ht(e.url,r.url)||!W(e.queryParams,r.queryParams);case"always":return!0;case"paramsOrQueryParamsChange":return!Sn(e,r)||!W(e.queryParams,r.queryParams);default:return!Sn(e,r)}}function Qt(e,r,t){let n=At(e),o=e.value;Object.entries(n).forEach(([i,s])=>{o.component?r?Qt(s,r.children.getContext(i),t):Qt(s,null,t):Qt(s,r,t)}),o.component?r&&r.outlet&&r.outlet.isActivated?t.canDeactivateChecks.push(new Dt(r.outlet.component,o)):t.canDeactivateChecks.push(new Dt(null,o)):t.canDeactivateChecks.push(new Dt(null,o))}function se(e){return typeof e=="function"}function na(e){return typeof e=="boolean"}function ra(e){return e&&se(e.canLoad)}function oa(e){return e&&se(e.canActivate)}function ia(e){return e&&se(e.canActivateChild)}function aa(e){return e&&se(e.canDeactivate)}function sa(e){return e&&se(e.canMatch)}function Io(e){return e instanceof nr||e?.name==="EmptyError"}var Ie=Symbol("INITIAL_VALUE");function Ut(){return V(e=>rr(e.map(r=>r.pipe(Rt(1),cr(Ie)))).pipe(C(r=>{for(let t of r)if(t!==!0){if(t===Ie)return Ie;if(t===!1||ca(t))return t}return!0}),wt(r=>r!==Ie),Rt(1)))}function ca(e){return Nt(e)||e instanceof Lt}function So(e){return e.aborted?f(void 0).pipe(Rt(1)):new tr(r=>{let t=()=>{r.next(),r.complete()};return e.addEventListener("abort",t),()=>e.removeEventListener("abort",t)})}function Eo(e){return xt(So(e))}function la(e){return J(r=>{let{targetSnapshot:t,currentSnapshot:n,guards:{canActivateChecks:o,canDeactivateChecks:i}}=r;return i.length===0&&o.length===0?f(E(m({},r),{guardsResult:!0})):da(i,t,n).pipe(J(s=>s&&na(s)?ua(t,o,e):f(s)),C(s=>E(m({},r),{guardsResult:s})))})}function da(e,r,t){return N(e).pipe(J(n=>ga(n.component,n.route,t,r)),tt(n=>n!==!0,!0))}function ua(e,r,t){return N(r).pipe(on(n=>or(pa(n.route.parent,t),ma(n.route,t),fa(e,n.path),ha(e,n.route))),tt(n=>n!==!0,!0))}function ma(e,r){return e!==null&&r&&r(new ze(e)),f(!0)}function pa(e,r){return e!==null&&r&&r(new Fe(e)),f(!0)}function ha(e,r){let t=r.routeConfig?r.routeConfig.canActivate:null;if(!t||t.length===0)return f(!0);let n=t.map(o=>fe(()=>{let i=r._environmentInjector,s=jt(o,i),a=oa(s)?s.canActivate(r,e):O(i,()=>s(r,e));return vt(a).pipe(tt())}));return f(n).pipe(Ut())}function fa(e,r){let t=r[r.length-1],o=r.slice(0,r.length-1).reverse().map(i=>Ji(i)).filter(i=>i!==null).map(i=>fe(()=>{let s=i.guards.map(a=>{let c=i.node._environmentInjector,l=jt(a,c),u=ia(l)?l.canActivateChild(t,e):O(c,()=>l(t,e));return vt(u).pipe(tt())});return f(s).pipe(Ut())}));return f(o).pipe(Ut())}function ga(e,r,t,n){let o=r&&r.routeConfig?r.routeConfig.canDeactivate:null;if(!o||o.length===0)return f(!0);let i=o.map(s=>{let a=r._environmentInjector,c=jt(s,a),l=aa(c)?c.canDeactivate(e,r,t,n):O(a,()=>c(e,r,t,n));return vt(l).pipe(tt())});return f(i).pipe(Ut())}function ba(e,r,t,n,o){let i=r.canLoad;if(i===void 0||i.length===0)return f(!0);let s=i.map(a=>{let c=jt(a,e),l=ra(c)?c.canLoad(r,t):O(e,()=>c(r,t)),u=vt(l);return o?u.pipe(Eo(o)):u});return f(s).pipe(Ut(),Mo(n))}function Mo(e){return Jn(j(r=>{if(typeof r!="boolean")throw qe(e,r)}),C(r=>r===!0))}function va(e,r,t,n,o,i){let s=r.canMatch;if(!s||s.length===0)return f(!0);let a=s.map(c=>{let l=jt(c,e),u=sa(l)?l.canMatch(r,t,o):O(e,()=>l(r,t,o));return vt(u).pipe(Eo(i))});return f(a).pipe(Ut(),Mo(n))}var K=class e extends Error{segmentGroup;constructor(r){super(),this.segmentGroup=r||null,Object.setPrototypeOf(this,e.prototype)}},re=class e extends Error{urlTree;constructor(r){super(),this.urlTree=r,Object.setPrototypeOf(this,e.prototype)}};function ya(e){throw new _(4e3,!1)}function _a(e){throw xo(!1,R.GuardRejected)}var An=class{urlSerializer;urlTree;constructor(r,t){this.urlSerializer=r,this.urlTree=t}async lineralizeSegments(r,t){let n=[],o=t.root;for(;;){if(n=n.concat(o.segments),o.numberOfChildren===0)return n;if(o.numberOfChildren>1||!o.children[h])throw ya(`${r.redirectTo}`);o=o.children[h]}}async applyRedirectCommands(r,t,n,o,i){let s=await wa(t,o,i);if(s instanceof L)throw new re(s);let a=this.applyRedirectCreateUrlTree(s,this.urlSerializer.parse(s),r,n);if(s[0]==="/")throw new re(a);return a}applyRedirectCreateUrlTree(r,t,n,o){let i=this.createSegmentGroup(r,t.root,n,o);return new L(i,this.createQueryParams(t.queryParams,this.urlTree.queryParams),t.fragment)}createQueryParams(r,t){let n={};return Object.entries(r).forEach(([o,i])=>{if(typeof i=="string"&&i[0]===":"){let a=i.substring(1);n[o]=t[a]}else n[o]=i}),n}createSegmentGroup(r,t,n,o){let i=this.createSegments(r,t.segments,n,o),s={};return Object.entries(t.children).forEach(([a,c])=>{s[a]=this.createSegmentGroup(r,c,n,o)}),new g(i,s)}createSegments(r,t,n,o){return t.map(i=>i.path[0]===":"?this.findPosParam(r,i,o):this.findOrReturn(i,n))}findPosParam(r,t,n){let o=n[t.path.substring(1)];if(!o)throw new _(4001,!1);return o}findOrReturn(r,t){let n=0;for(let o of t){if(o.path===r.path)return t.splice(n),o;n++}return r}};function wa(e,r,t){if(typeof e=="string")return Promise.resolve(e);let n=e;return Ae(vt(O(t,()=>n(r))))}function Ra(e,r){return e.providers&&!e._injector&&(e._injector=mn(e.providers,r,`Route: ${e.path}`)),e._injector??r}function z(e){return e.outlet||h}function xa(e,r){let t=e.filter(n=>z(n)===r);return t.push(...e.filter(n=>z(n)!==r)),t}var Tn={matched:!1,consumedSegments:[],remainingSegments:[],parameters:{},positionalParamSegments:{}};function Ao(e){return{routeConfig:e.routeConfig,url:e.url,params:e.params,queryParams:e.queryParams,fragment:e.fragment,data:e.data,outlet:e.outlet,title:e.title,paramMap:e.paramMap,queryParamMap:e.queryParamMap}}function Ca(e,r,t,n,o,i,s){let a=To(e,r,t);if(!a.matched)return f(a);let c=Ao(i(a));return n=Ra(r,n),va(n,r,t,o,c,s).pipe(C(l=>l===!0?a:m({},Tn)))}function To(e,r,t){if(r.path==="")return r.pathMatch==="full"&&(e.hasChildren()||t.length>0)?m({},Tn):{matched:!0,consumedSegments:[],remainingSegments:t,parameters:{},positionalParamSegments:{}};let o=(r.matcher||Jr)(t,e,r);if(!o)return m({},Tn);let i={};Object.entries(o.posParams??{}).forEach(([a,c])=>{i[a]=c.path});let s=o.consumed.length>0?m(m({},i),o.consumed[o.consumed.length-1].parameters):i;return{matched:!0,consumedSegments:o.consumed,remainingSegments:t.slice(o.consumed.length),parameters:s,positionalParamSegments:o.posParams??{}}}function Yr(e,r,t,n,o){return t.length>0&&Ea(e,t,n,o)?{segmentGroup:new g(r,Sa(n,new g(t,e.children))),slicedSegments:[]}:t.length===0&&Ma(e,t,n)?{segmentGroup:new g(e.segments,Ia(e,t,n,e.children)),slicedSegments:t}:{segmentGroup:new g(e.segments,e.children),slicedSegments:t}}function Ia(e,r,t,n){let o={};for(let i of t)if(Ze(e,r,i)&&!n[z(i)]){let s=new g([],{});o[z(i)]=s}return m(m({},n),o)}function Sa(e,r){let t={};t[h]=r;for(let n of e)if(n.path===""&&z(n)!==h){let o=new g([],{});t[z(n)]=o}return t}function Ea(e,r,t,n){return t.some(o=>!Ze(e,r,o)||!(z(o)!==h)?!1:!(n!==void 0&&z(o)===n))}function Ma(e,r,t){return t.some(n=>Ze(e,r,n))}function Ze(e,r,t){return(e.hasChildren()||r.length>0)&&t.pathMatch==="full"?!1:t.path===""}function Aa(e,r,t){return r.length===0&&!e.children[t]}var Dn=class{};async function Ta(e,r,t,n,o,i,s="emptyOnly",a){return new Nn(e,r,t,n,o,s,i,a).recognize()}var Da=31,Nn=class{injector;configLoader;rootComponentType;config;urlTree;paramsInheritanceStrategy;urlSerializer;abortSignal;applyRedirects;absoluteRedirectCount=0;allowRedirects=!0;constructor(r,t,n,o,i,s,a,c){this.injector=r,this.configLoader=t,this.rootComponentType=n,this.config=o,this.urlTree=i,this.paramsInheritanceStrategy=s,this.urlSerializer=a,this.abortSignal=c,this.applyRedirects=new An(this.urlSerializer,this.urlTree)}noMatchError(r){return new _(4002,`'${r.segmentGroup}'`)}async recognize(){let r=Yr(this.urlTree.root,[],[],this.config).segmentGroup,{children:t,rootSnapshot:n}=await this.match(r),o=new A(n,t),i=new ee("",o),s=mo(n,[],this.urlTree.queryParams,this.urlTree.fragment);return s.queryParams=this.urlTree.queryParams,i.url=this.urlSerializer.serialize(s),{state:i,tree:s}}async match(r){let t=new Pt([],Object.freeze({}),Object.freeze(m({},this.urlTree.queryParams)),this.urlTree.fragment,Object.freeze({}),h,this.rootComponentType,null,{},this.injector);try{return{children:await this.processSegmentGroup(this.injector,this.config,r,h,t),rootSnapshot:t}}catch(n){if(n instanceof re)return this.urlTree=n.urlTree,this.match(n.urlTree.root);throw n instanceof K?this.noMatchError(n):n}}async processSegmentGroup(r,t,n,o,i){if(n.segments.length===0&&n.hasChildren())return this.processChildren(r,t,n,i);let s=await this.processSegment(r,t,n,n.segments,o,!0,i);return s instanceof A?[s]:[]}async processChildren(r,t,n,o){let i=[];for(let c of Object.keys(n.children))c==="primary"?i.unshift(c):i.push(c);let s=[];for(let c of i){let l=n.children[c],u=xa(t,c),p=await this.processSegmentGroup(r,u,l,c,o);s.push(...p)}let a=Do(s);return Na(a),a}async processSegment(r,t,n,o,i,s,a){for(let c of t)try{return await this.processSegmentAgainstRoute(c._injector??r,t,c,n,o,i,s,a)}catch(l){if(l instanceof K||Io(l))continue;throw l}if(Aa(n,o,i))return new Dn;throw new K(n)}async processSegmentAgainstRoute(r,t,n,o,i,s,a,c){if(z(n)!==s&&(s===h||!Ze(o,i,n)))throw new K(o);if(n.redirectTo===void 0)return this.matchSegmentAgainstRoute(r,o,n,i,s,c);if(this.allowRedirects&&a)return this.expandSegmentAgainstRouteUsingRedirect(r,o,t,n,i,s,c);throw new K(o)}async expandSegmentAgainstRouteUsingRedirect(r,t,n,o,i,s,a){let{matched:c,parameters:l,consumedSegments:u,positionalParamSegments:p,remainingSegments:y}=To(t,o,i);if(!c)throw new K(t);typeof o.redirectTo=="string"&&o.redirectTo[0]==="/"&&(this.absoluteRedirectCount++,this.absoluteRedirectCount>Da&&(this.allowRedirects=!1));let H=this.createSnapshot(r,o,i,l,a);if(this.abortSignal.aborted)throw new Error(this.abortSignal.reason);let S=await this.applyRedirects.applyRedirectCommands(u,o.redirectTo,p,Ao(H),r),x=await this.applyRedirects.lineralizeSegments(o,S);return this.processSegment(r,n,t,x.concat(y),s,!1,a)}createSnapshot(r,t,n,o,i){let s=new Pt(n,o,Object.freeze(m({},this.urlTree.queryParams)),this.urlTree.fragment,ka(t),z(t),t.component??t._loadedComponent??null,t,Pa(t),r),a=On(s,i,this.paramsInheritanceStrategy);return s.params=Object.freeze(a.params),s.data=Object.freeze(a.data),s}async matchSegmentAgainstRoute(r,t,n,o,i,s){if(this.abortSignal.aborted)throw new Error(this.abortSignal.reason);let a=ct=>this.createSnapshot(r,n,ct.consumedSegments,ct.parameters,s),c=await Ae(Ca(t,n,o,r,this.urlSerializer,a,this.abortSignal));if(n.path==="**"&&(t.children={}),!c?.matched)throw new K(t);r=n._injector??r;let{routes:l}=await this.getChildConfig(r,n,o),u=n._loadedInjector??r,{parameters:p,consumedSegments:y,remainingSegments:H}=c,S=this.createSnapshot(r,n,y,p,s),{segmentGroup:x,slicedSegments:q}=Yr(t,y,H,l,i);if(q.length===0&&x.hasChildren()){let ct=await this.processChildren(u,l,x,S);return new A(S,ct)}if(l.length===0&&q.length===0)return new A(S,[]);let _t=z(n)===i,Z=await this.processSegment(u,l,x,q,_t?h:i,!0,S);return new A(S,Z instanceof A?[Z]:[])}async getChildConfig(r,t,n){if(t.children)return{routes:t.children,injector:r};if(t.loadChildren){if(t._loadedRoutes!==void 0){let i=t._loadedNgModuleFactory;return i&&!t._loadedInjector&&(t._loadedInjector=i.create(r).injector),{routes:t._loadedRoutes,injector:t._loadedInjector}}if(this.abortSignal.aborted)throw new Error(this.abortSignal.reason);if(await Ae(ba(r,t,n,this.urlSerializer,this.abortSignal))){let i=await this.configLoader.loadChildren(r,t);return t._loadedRoutes=i.routes,t._loadedInjector=i.injector,t._loadedNgModuleFactory=i.factory,i}throw _a(t)}return{routes:[],injector:r}}};function Na(e){e.sort((r,t)=>r.value.outlet===h?-1:t.value.outlet===h?1:r.value.outlet.localeCompare(t.value.outlet))}function Oa(e){let r=e.value.routeConfig;return r&&r.path===""}function Do(e){let r=[],t=new Set;for(let n of e){if(!Oa(n)){r.push(n);continue}let o=r.find(i=>n.value.routeConfig===i.value.routeConfig);o!==void 0?(o.children.push(...n.children),t.add(o)):r.push(n)}for(let n of t){let o=Do(n.children);r.push(new A(n.value,o))}return r.filter(n=>!t.has(n))}function ka(e){return e.data||{}}function Pa(e){return e.resolve||{}}function La(e,r,t,n,o,i,s){return J(async a=>{let{state:c,tree:l}=await Ta(e,r,t,n,a.extractedUrl,o,i,s);return E(m({},a),{targetSnapshot:c,urlAfterRedirects:l})})}function Ua(e){return J(r=>{let{targetSnapshot:t,guards:{canActivateChecks:n}}=r;if(!n.length)return f(r);let o=new Set(n.map(a=>a.route)),i=new Set;for(let a of o)if(!i.has(a))for(let c of No(a))i.add(c);let s=0;return N(i).pipe(on(a=>o.has(a)?Fa(a,t,e):(a.data=On(a,a.parent,e).resolve,f(void 0))),j(()=>s++),an(1),J(a=>s===i.size?f(r):D))})}function No(e){let r=e.children.map(t=>No(t)).flat();return[e,...r]}function Fa(e,r,t){let n=e.routeConfig,o=e._resolve;return n?.title!==void 0&&!_o(n)&&(o[oe]=n.title),fe(()=>(e.data=On(e,e.parent,t).resolve,ja(o,e,r).pipe(C(i=>(e._resolvedData=i,e.data=m(m({},e.data),i),null)))))}function ja(e,r,t){let n=yn(e);if(n.length===0)return f({});let o={};return N(n).pipe(J(i=>za(e[i],r,t).pipe(tt(),j(s=>{if(s instanceof Lt)throw qe(new it,s);o[i]=s}))),an(1),C(()=>o),rn(i=>Io(i)?D:er(i)))}function za(e,r,t){let n=r._environmentInjector,o=jt(e,n),i=o.resolve?o.resolve(r,t):O(n,()=>o(r,t));return vt(i)}function Xr(e){return V(r=>{let t=e(r);return t?N(t).pipe(C(()=>r)):f(r)})}var Un=(()=>{class e{buildTitle(t){let n,o=t.root;for(;o!==void 0;)n=this.getResolvedTitleForRoute(o)??n,o=o.children.find(i=>i.outlet===h);return n}getResolvedTitleForRoute(t){return t.data[oe]}static \u0275fac=function(n){return new(n||e)};static \u0275prov=b({token:e,factory:()=>d(Oo),providedIn:"root"})}return e})(),Oo=(()=>{class e extends Un{title;constructor(t){super(),this.title=t}updateTitle(t){let n=this.buildTitle(t);n!==void 0&&this.title.setTitle(n)}static \u0275fac=function(n){return new(n||e)(sn(Hr))};static \u0275prov=b({token:e,factory:e.\u0275fac,providedIn:"root"})}return e})(),ce=new v("",{factory:()=>({})}),le=new v(""),ko=(()=>{class e{componentLoaders=new WeakMap;childrenLoaders=new WeakMap;onLoadStartListener;onLoadEndListener;compiler=d(Or);async loadComponent(t,n){if(this.componentLoaders.get(n))return this.componentLoaders.get(n);if(n._loadedComponent)return Promise.resolve(n._loadedComponent);this.onLoadStartListener&&this.onLoadStartListener(n);let o=(async()=>{try{let i=await eo(O(t,()=>n.loadComponent())),s=await Uo(Lo(i));return this.onLoadEndListener&&this.onLoadEndListener(n),n._loadedComponent=s,s}finally{this.componentLoaders.delete(n)}})();return this.componentLoaders.set(n,o),o}loadChildren(t,n){if(this.childrenLoaders.get(n))return this.childrenLoaders.get(n);if(n._loadedRoutes)return Promise.resolve({routes:n._loadedRoutes,injector:n._loadedInjector});this.onLoadStartListener&&this.onLoadStartListener(n);let o=(async()=>{try{let i=await Po(n,this.compiler,t,this.onLoadEndListener);return n._loadedRoutes=i.routes,n._loadedInjector=i.injector,n._loadedNgModuleFactory=i.factory,i}finally{this.childrenLoaders.delete(n)}})();return this.childrenLoaders.set(n,o),o}static \u0275fac=function(n){return new(n||e)};static \u0275prov=b({token:e,factory:e.\u0275fac,providedIn:"root"})}return e})();async function Po(e,r,t,n){let o=await eo(O(t,()=>e.loadChildren())),i=await Uo(Lo(o)),s;i instanceof wr||Array.isArray(i)?s=i:s=await r.compileModuleAsync(i),n&&n(e);let a,c,l=!1,u;return Array.isArray(s)?(c=s,l=!0):(a=s.create(t).injector,u=s,c=a.get(le,[],{optional:!0,self:!0}).flat()),{routes:c.map(Ln),injector:a,factory:u}}function Ba(e){return e&&typeof e=="object"&&"default"in e}function Lo(e){return Ba(e)?e.default:e}async function Uo(e){return e}var Qe=(()=>{class e{static \u0275fac=function(n){return new(n||e)};static \u0275prov=b({token:e,factory:()=>d($a),providedIn:"root"})}return e})(),$a=(()=>{class e{shouldProcessUrl(t){return!0}extract(t){return t}merge(t,n){return t}static \u0275fac=function(n){return new(n||e)};static \u0275prov=b({token:e,factory:e.\u0275fac,providedIn:"root"})}return e})(),Fo=new v("");var Ha=()=>{},jo=new v(""),zo=(()=>{class e{currentNavigation=be(null,{equal:()=>!1});currentTransition=null;lastSuccessfulNavigation=be(null);events=new Q;transitionAbortWithErrorSubject=new Q;configLoader=d(ko);environmentInjector=d(lt);destroyRef=d(cn);urlSerializer=d(ie);rootContexts=d(Ft);location=d(Re);inputBindingEnabled=d(We,{optional:!0})!==null;titleStrategy=d(Un);options=d(ce,{optional:!0})||{};paramsInheritanceStrategy=this.options.paramsInheritanceStrategy||"emptyOnly";urlHandlingStrategy=d(Qe);createViewTransition=d(Fo,{optional:!0});navigationErrorHandler=d(jo,{optional:!0});navigationId=0;get hasRequestedNavigation(){return this.navigationId!==0}transitions;afterPreactivation=()=>f(void 0);rootComponentType=null;destroyed=!1;constructor(){let t=o=>this.events.next(new Le(o)),n=o=>this.events.next(new Ue(o));this.configLoader.onLoadEndListener=n,this.configLoader.onLoadStartListener=t,this.destroyRef.onDestroy(()=>{this.destroyed=!0})}complete(){this.transitions?.complete()}handleNavigationRequest(t){let n=++this.navigationId;pt(()=>{this.transitions?.next(E(m({},t),{extractedUrl:this.urlHandlingStrategy.extract(t.rawUrl),targetSnapshot:null,targetRouterState:null,guards:{canActivateChecks:[],canDeactivateChecks:[]},guardsResult:null,id:n,routesRecognizeHandler:{},beforeActivateHandler:{}}))})}setupNavigations(t){return this.transitions=new I(null),this.transitions.pipe(wt(n=>n!==null),V(n=>{let o=!1,i=new AbortController,s=()=>!o&&this.currentTransition?.id===n.id;return f(n).pipe(V(a=>{if(this.navigationId>n.id)return this.cancelNavigationTransition(n,"",R.SupersededByNewNavigation),D;this.currentTransition=n;let c=this.lastSuccessfulNavigation();this.currentNavigation.set({id:a.id,initialUrl:a.rawUrl,extractedUrl:a.extractedUrl,targetBrowserUrl:typeof a.extras.browserUrl=="string"?this.urlSerializer.parse(a.extras.browserUrl):a.extras.browserUrl,trigger:a.source,extras:a.extras,previousNavigation:c?E(m({},c),{previousNavigation:null}):null,abort:()=>i.abort(),routesRecognizeHandler:a.routesRecognizeHandler,beforeActivateHandler:a.beforeActivateHandler});let l=!t.navigated||this.isUpdatingInternalState()||this.isUpdatedBrowserUrl(),u=a.extras.onSameUrlNavigation??t.onSameUrlNavigation;if(!l&&u!=="reload")return this.events.next(new X(a.id,this.urlSerializer.serialize(a.rawUrl),"",Yt.IgnoredSameUrlNavigation)),a.resolve(!1),D;if(this.urlHandlingStrategy.shouldProcessUrl(a.rawUrl))return f(a).pipe(V(p=>(this.events.next(new gt(p.id,this.urlSerializer.serialize(p.extractedUrl),p.source,p.restoredState)),p.id!==this.navigationId?D:Promise.resolve(p))),La(this.environmentInjector,this.configLoader,this.rootComponentType,t.config,this.urlSerializer,this.paramsInheritanceStrategy,i.signal),j(p=>{n.targetSnapshot=p.targetSnapshot,n.urlAfterRedirects=p.urlAfterRedirects,this.currentNavigation.update(y=>(y.finalUrl=p.urlAfterRedirects,y)),this.events.next(new Jt)}),V(p=>N(n.routesRecognizeHandler.deferredHandle??f(void 0)).pipe(C(()=>p))),j(()=>{let p=new Xt(a.id,this.urlSerializer.serialize(a.extractedUrl),this.urlSerializer.serialize(a.urlAfterRedirects),a.targetSnapshot);this.events.next(p)}));if(l&&this.urlHandlingStrategy.shouldProcessUrl(a.currentRawUrl)){let{id:p,extractedUrl:y,source:H,restoredState:S,extras:x}=a,q=new gt(p,this.urlSerializer.serialize(y),H,S);this.events.next(q);let _t=vo(this.rootComponentType,this.environmentInjector).snapshot;return this.currentTransition=n=E(m({},a),{targetSnapshot:_t,urlAfterRedirects:y,extras:E(m({},x),{skipLocationChange:!1,replaceUrl:!1})}),this.currentNavigation.update(Z=>(Z.finalUrl=y,Z)),f(n)}else return this.events.next(new X(a.id,this.urlSerializer.serialize(a.extractedUrl),"",Yt.IgnoredByUrlHandlingStrategy)),a.resolve(!1),D}),C(a=>{let c=new Ne(a.id,this.urlSerializer.serialize(a.extractedUrl),this.urlSerializer.serialize(a.urlAfterRedirects),a.targetSnapshot);return this.events.next(c),this.currentTransition=n=E(m({},a),{guards:Xi(a.targetSnapshot,a.currentSnapshot,this.rootContexts)}),n}),la(a=>this.events.next(a)),V(a=>{if(n.guardsResult=a.guardsResult,a.guardsResult&&typeof a.guardsResult!="boolean")throw qe(this.urlSerializer,a.guardsResult);let c=new Oe(a.id,this.urlSerializer.serialize(a.extractedUrl),this.urlSerializer.serialize(a.urlAfterRedirects),a.targetSnapshot,!!a.guardsResult);if(this.events.next(c),!s())return D;if(!a.guardsResult)return this.cancelNavigationTransition(a,"",R.GuardRejected),D;if(a.guards.canActivateChecks.length===0)return f(a);let l=new ke(a.id,this.urlSerializer.serialize(a.extractedUrl),this.urlSerializer.serialize(a.urlAfterRedirects),a.targetSnapshot);if(this.events.next(l),!s())return D;let u=!1;return f(a).pipe(Ua(this.paramsInheritanceStrategy),j({next:()=>{u=!0;let p=new Pe(a.id,this.urlSerializer.serialize(a.extractedUrl),this.urlSerializer.serialize(a.urlAfterRedirects),a.targetSnapshot);this.events.next(p)},complete:()=>{u||this.cancelNavigationTransition(a,"",R.NoDataFromResolver)}}))}),Xr(a=>{let c=u=>{let p=[];if(u.routeConfig?._loadedComponent)u.component=u.routeConfig?._loadedComponent;else if(u.routeConfig?.loadComponent){let y=u._environmentInjector;p.push(this.configLoader.loadComponent(y,u.routeConfig).then(H=>{u.component=H}))}for(let y of u.children)p.push(...c(y));return p},l=c(a.targetSnapshot.root);return l.length===0?f(a):N(Promise.all(l).then(()=>a))}),Xr(()=>this.afterPreactivation()),V(()=>{let{currentSnapshot:a,targetSnapshot:c}=n,l=this.createViewTransition?.(this.environmentInjector,a.root,c.root);return l?N(l).pipe(C(()=>n)):f(n)}),Rt(1),V(a=>{let c=Zi(t.routeReuseStrategy,a.targetSnapshot,a.currentRouterState);this.currentTransition=n=a=E(m({},a),{targetRouterState:c}),this.currentNavigation.update(u=>(u.targetRouterState=c,u)),this.events.next(new Ot);let l=n.beforeActivateHandler.deferredHandle;return l?N(l.then(()=>a)):f(a)}),j(a=>{new Mn(t.routeReuseStrategy,n.targetRouterState,n.currentRouterState,c=>this.events.next(c),this.inputBindingEnabled).activate(this.rootContexts),s()&&(o=!0,this.currentNavigation.update(c=>(c.abort=Ha,c)),this.lastSuccessfulNavigation.set(pt(this.currentNavigation)),this.events.next(new Y(a.id,this.urlSerializer.serialize(a.extractedUrl),this.urlSerializer.serialize(a.urlAfterRedirects))),this.titleStrategy?.updateTitle(a.targetRouterState.snapshot),a.resolve(!0))}),xt(So(i.signal).pipe(wt(()=>!o&&!n.targetRouterState),j(()=>{this.cancelNavigationTransition(n,i.signal.reason+"",R.Aborted)}))),j({complete:()=>{o=!0}}),xt(this.transitionAbortWithErrorSubject.pipe(j(a=>{throw a}))),ar(()=>{i.abort(),o||this.cancelNavigationTransition(n,"",R.SupersededByNewNavigation),this.currentTransition?.id===n.id&&(this.currentNavigation.set(null),this.currentTransition=null)}),rn(a=>{if(o=!0,this.destroyed)return n.resolve(!1),D;if(Co(a))this.events.next(new P(n.id,this.urlSerializer.serialize(n.extractedUrl),a.message,a.cancellationCode)),Yi(a)?this.events.next(new kt(a.url,a.navigationBehaviorOptions)):n.resolve(!1);else{let c=new bt(n.id,this.urlSerializer.serialize(n.extractedUrl),a,n.targetSnapshot??void 0);try{let l=O(this.environmentInjector,()=>this.navigationErrorHandler?.(c));if(l instanceof Lt){let{message:u,cancellationCode:p}=qe(this.urlSerializer,l);this.events.next(new P(n.id,this.urlSerializer.serialize(n.extractedUrl),u,p)),this.events.next(new kt(l.redirectTo,l.navigationBehaviorOptions))}else throw this.events.next(c),a}catch(l){this.options.resolveNavigationPromiseOnError?n.resolve(!1):n.reject(l)}}return D}))}))}cancelNavigationTransition(t,n,o){let i=new P(t.id,this.urlSerializer.serialize(t.extractedUrl),n,o);this.events.next(i),t.resolve(!1)}isUpdatingInternalState(){return this.currentTransition?.extractedUrl.toString()!==this.currentTransition?.currentUrlTree.toString()}isUpdatedBrowserUrl(){let t=this.urlHandlingStrategy.extract(this.urlSerializer.parse(this.location.path(!0))),n=pt(this.currentNavigation),o=n?.targetBrowserUrl??n?.extractedUrl;return t.toString()!==o?.toString()&&!n?.extras.skipLocationChange}static \u0275fac=function(n){return new(n||e)};static \u0275prov=b({token:e,factory:e.\u0275fac,providedIn:"root"})}return e})();function qa(e){return e!==Zt}var Bo=new v("");var $o=(()=>{class e{static \u0275fac=function(n){return new(n||e)};static \u0275prov=b({token:e,factory:()=>d(Va),providedIn:"root"})}return e})(),Ge=class{shouldDetach(r){return!1}store(r,t){}shouldAttach(r){return!1}retrieve(r){return null}shouldReuseRoute(r,t){return r.routeConfig===t.routeConfig}shouldDestroyInjector(r){return!0}},Va=(()=>{class e extends Ge{static \u0275fac=(()=>{let t;return function(o){return(t||(t=dn(e)))(o||e)}})();static \u0275prov=b({token:e,factory:e.\u0275fac,providedIn:"root"})}return e})(),Fn=(()=>{class e{urlSerializer=d(ie);options=d(ce,{optional:!0})||{};canceledNavigationResolution=this.options.canceledNavigationResolution||"replace";location=d(Re);urlHandlingStrategy=d(Qe);urlUpdateStrategy=this.options.urlUpdateStrategy||"deferred";currentUrlTree=new L;getCurrentUrlTree(){return this.currentUrlTree}rawUrlTree=this.currentUrlTree;getRawUrlTree(){return this.rawUrlTree}createBrowserPath({finalUrl:t,initialUrl:n,targetBrowserUrl:o}){let i=t!==void 0?this.urlHandlingStrategy.merge(t,n):n,s=o??i;return s instanceof L?this.urlSerializer.serialize(s):s}routerUrlState(t){return t?.targetBrowserUrl===void 0||t?.finalUrl===void 0?{}:{\u0275routerUrl:this.urlSerializer.serialize(t.finalUrl)}}commitTransition({targetRouterState:t,finalUrl:n,initialUrl:o}){n&&t?(this.currentUrlTree=n,this.rawUrlTree=this.urlHandlingStrategy.merge(n,o),this.routerState=t):this.rawUrlTree=o}routerState=vo(null,d(lt));getRouterState(){return this.routerState}_stateMemento=this.createStateMemento();get stateMemento(){return this._stateMemento}updateStateMemento(){this._stateMemento=this.createStateMemento()}createStateMemento(){return{rawUrlTree:this.rawUrlTree,currentUrlTree:this.currentUrlTree,routerState:this.routerState}}restoredState(){return this.location.getState()}static \u0275fac=function(n){return new(n||e)};static \u0275prov=b({token:e,factory:()=>d(Ga),providedIn:"root"})}return e})(),Ga=(()=>{class e extends Fn{currentPageId=0;lastSuccessfulId=-1;get browserPageId(){return this.canceledNavigationResolution!=="computed"?this.currentPageId:this.restoredState()?.\u0275routerPageId??this.currentPageId}registerNonRouterCurrentEntryChangeListener(t){return this.location.subscribe(n=>{n.type==="popstate"&&setTimeout(()=>{t(n.url,n.state,"popstate",{replaceUrl:!0})})})}handleRouterEvent(t,n){t instanceof gt?this.updateStateMemento():t instanceof X?this.commitTransition(n):t instanceof Xt?this.urlUpdateStrategy==="eager"&&(n.extras.skipLocationChange||this.setBrowserUrl(this.createBrowserPath(n),n)):t instanceof Ot?(this.commitTransition(n),this.urlUpdateStrategy==="deferred"&&!n.extras.skipLocationChange&&this.setBrowserUrl(this.createBrowserPath(n),n)):t instanceof P&&!bo(t)?this.restoreHistory(n):t instanceof bt?this.restoreHistory(n,!0):t instanceof Y&&(this.lastSuccessfulId=t.id,this.currentPageId=this.browserPageId)}setBrowserUrl(t,n){let{extras:o,id:i}=n,{replaceUrl:s,state:a}=o;if(this.location.isCurrentPathEqualTo(t)||s){let c=this.browserPageId,l=m(m({},a),this.generateNgRouterState(i,c,n));this.location.replaceState(t,"",l)}else{let c=m(m({},a),this.generateNgRouterState(i,this.browserPageId+1,n));this.location.go(t,"",c)}}restoreHistory(t,n=!1){if(this.canceledNavigationResolution==="computed"){let o=this.browserPageId,i=this.currentPageId-o;i!==0?this.location.historyGo(i):this.getCurrentUrlTree()===t.finalUrl&&i===0&&(this.resetInternalState(t),this.resetUrlToCurrentUrlTree())}else this.canceledNavigationResolution==="replace"&&(n&&this.resetInternalState(t),this.resetUrlToCurrentUrlTree())}resetInternalState({finalUrl:t}){this.routerState=this.stateMemento.routerState,this.currentUrlTree=this.stateMemento.currentUrlTree,this.rawUrlTree=this.urlHandlingStrategy.merge(this.currentUrlTree,t??this.rawUrlTree)}resetUrlToCurrentUrlTree(){this.location.replaceState(this.urlSerializer.serialize(this.getRawUrlTree()),"",this.generateNgRouterState(this.lastSuccessfulId,this.currentPageId))}generateNgRouterState(t,n,o){return this.canceledNavigationResolution==="computed"?m({navigationId:t,\u0275routerPageId:n},this.routerUrlState(o)):m({navigationId:t},this.routerUrlState(o))}static \u0275fac=(()=>{let t;return function(o){return(t||(t=dn(e)))(o||e)}})();static \u0275prov=b({token:e,factory:e.\u0275fac,providedIn:"root"})}return e})();function jn(e,r){e.events.pipe(wt(t=>t instanceof Y||t instanceof P||t instanceof bt||t instanceof X),C(t=>t instanceof Y||t instanceof X?0:(t instanceof P?t.code===R.Redirect||t.code===R.SupersededByNewNavigation:!1)?2:1),wt(t=>t!==2),Rt(1)).subscribe(()=>{r()})}var Ke=(()=>{class e{get currentUrlTree(){return this.stateManager.getCurrentUrlTree()}get rawUrlTree(){return this.stateManager.getRawUrlTree()}disposed=!1;nonRouterCurrentEntryChangeSubscription;console=d(Rr);stateManager=d(Fn);options=d(ce,{optional:!0})||{};pendingTasks=d(ur);urlUpdateStrategy=this.options.urlUpdateStrategy||"deferred";navigationTransitions=d(zo);urlSerializer=d(ie);location=d(Re);urlHandlingStrategy=d(Qe);injector=d(lt);_events=new Q;get events(){return this._events}get routerState(){return this.stateManager.getRouterState()}navigated=!1;routeReuseStrategy=d($o);injectorCleanup=d(Bo,{optional:!0});onSameUrlNavigation=this.options.onSameUrlNavigation||"ignore";config=d(le,{optional:!0})?.flat()??[];componentInputBindingEnabled=!!d(We,{optional:!0});currentNavigation=this.navigationTransitions.currentNavigation.asReadonly();constructor(){this.resetConfig(this.config),this.navigationTransitions.setupNavigations(this).subscribe({error:t=>{}}),this.subscribeToNavigationEvents()}eventsSubscription=new Xn;subscribeToNavigationEvents(){let t=this.navigationTransitions.events.subscribe(n=>{try{let o=this.navigationTransitions.currentTransition,i=pt(this.navigationTransitions.currentNavigation);if(o!==null&&i!==null){if(this.stateManager.handleRouterEvent(n,i),n instanceof P&&n.code!==R.Redirect&&n.code!==R.SupersededByNewNavigation)this.navigated=!0;else if(n instanceof Y)this.navigated=!0,this.injectorCleanup?.(this.routeReuseStrategy,this.routerState,this.config);else if(n instanceof kt){let s=n.navigationBehaviorOptions,a=this.urlHandlingStrategy.merge(n.url,o.currentRawUrl),c=m({scroll:o.extras.scroll,browserUrl:o.extras.browserUrl,info:o.extras.info,skipLocationChange:o.extras.skipLocationChange,replaceUrl:o.extras.replaceUrl||this.urlUpdateStrategy==="eager"||qa(o.source)},s);this.scheduleNavigation(a,Zt,null,c,{resolve:o.resolve,reject:o.reject,promise:o.promise})}}Gi(n)&&this._events.next(n)}catch(o){this.navigationTransitions.transitionAbortWithErrorSubject.next(o)}});this.eventsSubscription.add(t)}resetRootComponentType(t){this.routerState.root.component=t,this.navigationTransitions.rootComponentType=t}initialNavigation(){this.setUpLocationChangeListener(),this.navigationTransitions.hasRequestedNavigation||this.navigateToSyncWithBrowser(this.location.path(!0),Zt,this.stateManager.restoredState(),{replaceUrl:!0})}setUpLocationChangeListener(){this.nonRouterCurrentEntryChangeSubscription??=this.stateManager.registerNonRouterCurrentEntryChangeListener((t,n,o,i)=>{this.navigateToSyncWithBrowser(t,o,n,i)})}navigateToSyncWithBrowser(t,n,o,i){let s=o?.navigationId?o:null,a=o?.\u0275routerUrl??t;if(o?.\u0275routerUrl&&(i=E(m({},i),{browserUrl:t})),o){let l=m({},o);delete l.navigationId,delete l.\u0275routerPageId,delete l.\u0275routerUrl,Object.keys(l).length!==0&&(i.state=l)}let c=this.parseUrl(a);this.scheduleNavigation(c,n,s,i).catch(l=>{this.disposed||this.injector.get(ln)(l)})}get url(){return this.serializeUrl(this.currentUrlTree)}getCurrentNavigation(){return pt(this.navigationTransitions.currentNavigation)}get lastSuccessfulNavigation(){return this.navigationTransitions.lastSuccessfulNavigation}resetConfig(t){this.config=t.map(Ln),this.navigated=!1}ngOnDestroy(){this.dispose()}dispose(){this._events.unsubscribe(),this.navigationTransitions.complete(),this.nonRouterCurrentEntryChangeSubscription?.unsubscribe(),this.nonRouterCurrentEntryChangeSubscription=void 0,this.disposed=!0,this.eventsSubscription.unsubscribe()}createUrlTree(t,n={}){let{relativeTo:o,queryParams:i,fragment:s,queryParamsHandling:a,preserveFragment:c}=n,l=c?this.currentUrlTree.fragment:s,u=null;switch(a??this.options.defaultQueryParamsHandling){case"merge":u=m(m({},this.currentUrlTree.queryParams),i);break;case"preserve":u=this.currentUrlTree.queryParams;break;default:u=i||null}u!==null&&(u=this.removeEmptyProps(u));let p;try{let y=o?o.snapshot:this.routerState.snapshot.root;p=po(y)}catch{(typeof t[0]!="string"||t[0][0]!=="/")&&(t=[]),p=this.currentUrlTree.root}return ho(p,t,u,l??null,this.urlSerializer)}navigateByUrl(t,n={skipLocationChange:!1}){let o=Nt(t)?t:this.parseUrl(t),i=this.urlHandlingStrategy.merge(o,this.rawUrlTree);return this.scheduleNavigation(i,Zt,null,n)}navigate(t,n={skipLocationChange:!1}){return Wa(t),this.navigateByUrl(this.createUrlTree(t,n),n)}serializeUrl(t){return this.urlSerializer.serialize(t)}parseUrl(t){try{return this.urlSerializer.parse(t)}catch{return this.console.warn(lr(4018,!1)),this.urlSerializer.parse("/")}}isActive(t,n){let o;if(n===!0?o=m({},ro):n===!1?o=m({},_n):o=m(m({},_n),n),Nt(t))return Gr(this.currentUrlTree,t,o);let i=this.parseUrl(t);return Gr(this.currentUrlTree,i,o)}removeEmptyProps(t){return Object.entries(t).reduce((n,[o,i])=>(i!=null&&(n[o]=i),n),{})}scheduleNavigation(t,n,o,i,s){if(this.disposed)return Promise.resolve(!1);let a,c,l;s?(a=s.resolve,c=s.reject,l=s.promise):l=new Promise((p,y)=>{a=p,c=y});let u=this.pendingTasks.add();return jn(this,()=>{queueMicrotask(()=>this.pendingTasks.remove(u))}),this.navigationTransitions.handleNavigationRequest({source:n,restoredState:o,currentUrlTree:this.currentUrlTree,currentRawUrl:this.currentUrlTree,rawUrl:t,extras:i,resolve:a,reject:c,promise:l,currentSnapshot:this.routerState.snapshot,currentRouterState:this.routerState}),l.catch(Promise.reject.bind(Promise))}static \u0275fac=function(n){return new(n||e)};static \u0275prov=b({token:e,factory:e.\u0275fac,providedIn:"root"})}return e})();function Wa(e){for(let r=0;r<e.length;r++)if(e[r]==null)throw new _(4008,!1)}var Ka=new v("");function zn(e,...r){return ge([{provide:le,multi:!0,useValue:e},[],{provide:at,useFactory:Ya},{provide:Cr,multi:!0,useFactory:Xa},r.map(t=>t.\u0275providers)])}function Ya(){return d(Ke).routerState.root}function Xa(){let e=d(dt);return r=>{let t=e.get(Ir);if(r!==t.components[0])return;let n=e.get(Ke),o=e.get(Ja);e.get(ts)===1&&n.initialNavigation(),e.get(es,null,{optional:!0})?.setUpPreloading(),e.get(Ka,null,{optional:!0})?.init(),n.resetRootComponentType(t.componentTypes[0]),o.closed||(o.next(),o.complete(),o.unsubscribe())}}var Ja=new v("",{factory:()=>new Q}),ts=new v("",{factory:()=>1});var es=new v("");function de(e){return e.buttons===0||e.detail===0}function ue(e){let r=e.touches&&e.touches[0]||e.changedTouches&&e.changedTouches[0];return!!r&&r.identifier===-1&&(r.radiusX==null||r.radiusX===1)&&(r.radiusY==null||r.radiusY===1)}var Bn;function Ho(){if(Bn==null){let e=typeof document<"u"?document.head:null;Bn=!!(e&&(e.createShadowRoot||e.attachShadow))}return Bn}function $n(e){if(Ho()){let r=e.getRootNode?e.getRootNode():null;if(typeof ShadowRoot<"u"&&ShadowRoot&&r instanceof ShadowRoot)return r}return null}function B(e){return e.composedPath?e.composedPath()[0]:e.target}var Hn;try{Hn=typeof Intl<"u"&&Intl.v8BreakIterator}catch{Hn=!1}var $=(()=>{class e{_platformId=d(fr);isBrowser=this._platformId?Fr(this._platformId):typeof document=="object"&&!!document;EDGE=this.isBrowser&&/(edge)/i.test(navigator.userAgent);TRIDENT=this.isBrowser&&/(msie|trident)/i.test(navigator.userAgent);BLINK=this.isBrowser&&!!(window.chrome||Hn)&&typeof CSS<"u"&&!this.EDGE&&!this.TRIDENT;WEBKIT=this.isBrowser&&/AppleWebKit/i.test(navigator.userAgent)&&!this.BLINK&&!this.EDGE&&!this.TRIDENT;IOS=this.isBrowser&&/iPad|iPhone|iPod/.test(navigator.userAgent)&&!("MSStream"in window);FIREFOX=this.isBrowser&&/(firefox|minefield)/i.test(navigator.userAgent);ANDROID=this.isBrowser&&/android/i.test(navigator.userAgent)&&!this.TRIDENT;SAFARI=this.isBrowser&&/safari/i.test(navigator.userAgent)&&this.WEBKIT;constructor(){}static \u0275fac=function(n){return new(n||e)};static \u0275prov=b({token:e,factory:e.\u0275fac,providedIn:"root"})}return e})();var me;function qo(){if(me==null&&typeof window<"u")try{window.addEventListener("test",null,Object.defineProperty({},"passive",{get:()=>me=!0}))}finally{me=me||!1}return me}function zt(e){return qo()?e:!!e.capture}function st(e){return e instanceof et?e.nativeElement:e}var Vo=new v("cdk-input-modality-detector-options"),Go={ignoreKeys:[18,17,224,91,16]},Wo=650,qn={passive:!0,capture:!0},Zo=(()=>{class e{_platform=d($);_listenerCleanups;modalityDetected;modalityChanged;get mostRecentModality(){return this._modality.value}_mostRecentTarget=null;_modality=new I(null);_options;_lastTouchMs=0;_onKeydown=t=>{this._options?.ignoreKeys?.some(n=>n===t.keyCode)||(this._modality.next("keyboard"),this._mostRecentTarget=B(t))};_onMousedown=t=>{Date.now()-this._lastTouchMs<Wo||(this._modality.next(de(t)?"keyboard":"mouse"),this._mostRecentTarget=B(t))};_onTouchstart=t=>{if(ue(t)){this._modality.next("keyboard");return}this._lastTouchMs=Date.now(),this._modality.next("touch"),this._mostRecentTarget=B(t)};constructor(){let t=d(k),n=d(G),o=d(Vo,{optional:!0});if(this._options=m(m({},Go),o),this.modalityDetected=this._modality.pipe(sr(1)),this.modalityChanged=this.modalityDetected.pipe(ir()),this._platform.isBrowser){let i=d(It).createRenderer(null,null);this._listenerCleanups=t.runOutsideAngular(()=>[i.listen(n,"keydown",this._onKeydown,qn),i.listen(n,"mousedown",this._onMousedown,qn),i.listen(n,"touchstart",this._onTouchstart,qn)])}}ngOnDestroy(){this._modality.complete(),this._listenerCleanups?.forEach(t=>t())}static \u0275fac=function(n){return new(n||e)};static \u0275prov=b({token:e,factory:e.\u0275fac,providedIn:"root"})}return e})(),pe=(function(e){return e[e.IMMEDIATE=0]="IMMEDIATE",e[e.EVENTUAL=1]="EVENTUAL",e})(pe||{}),Qo=new v("cdk-focus-monitor-default-options"),Ye=zt({passive:!0,capture:!0}),Vn=(()=>{class e{_ngZone=d(k);_platform=d($);_inputModalityDetector=d(Zo);_origin=null;_lastFocusOrigin=null;_windowFocused=!1;_windowFocusTimeoutId;_originTimeoutId;_originFromTouchInteraction=!1;_elementInfo=new Map;_monitoredElementCount=0;_rootNodeFocusListenerCount=new Map;_detectionMode;_windowFocusListener=()=>{this._windowFocused=!0,this._windowFocusTimeoutId=setTimeout(()=>this._windowFocused=!1)};_document=d(G);_stopInputModalityDetector=new Q;constructor(){let t=d(Qo,{optional:!0});this._detectionMode=t?.detectionMode||pe.IMMEDIATE}_rootNodeFocusAndBlurListener=t=>{let n=B(t);for(let o=n;o;o=o.parentElement)t.type==="focus"?this._onFocus(t,o):this._onBlur(t,o)};monitor(t,n=!1){let o=st(t);if(!this._platform.isBrowser||o.nodeType!==1)return f();let i=$n(o)||this._document,s=this._elementInfo.get(o);if(s)return n&&(s.checkChildren=!0),s.subject;let a={checkChildren:n,subject:new Q,rootNode:i};return this._elementInfo.set(o,a),this._registerGlobalListeners(a),a.subject}stopMonitoring(t){let n=st(t),o=this._elementInfo.get(n);o&&(o.subject.complete(),this._setClasses(n),this._elementInfo.delete(n),this._removeGlobalListeners(o))}focusVia(t,n,o){let i=st(t),s=this._document.activeElement;i===s?this._getClosestElementsInfo(i).forEach(([a,c])=>this._originChanged(a,n,c)):(this._setOrigin(n),typeof i.focus=="function"&&i.focus(o))}ngOnDestroy(){this._elementInfo.forEach((t,n)=>this.stopMonitoring(n))}_getWindow(){return this._document.defaultView||window}_getFocusOrigin(t){return this._origin?this._originFromTouchInteraction?this._shouldBeAttributedToTouch(t)?"touch":"program":this._origin:this._windowFocused&&this._lastFocusOrigin?this._lastFocusOrigin:t&&this._isLastInteractionFromInputLabel(t)?"mouse":"program"}_shouldBeAttributedToTouch(t){return this._detectionMode===pe.EVENTUAL||!!t?.contains(this._inputModalityDetector._mostRecentTarget)}_setClasses(t,n){t.classList.toggle("cdk-focused",!!n),t.classList.toggle("cdk-touch-focused",n==="touch"),t.classList.toggle("cdk-keyboard-focused",n==="keyboard"),t.classList.toggle("cdk-mouse-focused",n==="mouse"),t.classList.toggle("cdk-program-focused",n==="program")}_setOrigin(t,n=!1){this._ngZone.runOutsideAngular(()=>{if(this._origin=t,this._originFromTouchInteraction=t==="touch"&&n,this._detectionMode===pe.IMMEDIATE){clearTimeout(this._originTimeoutId);let o=this._originFromTouchInteraction?Wo:1;this._originTimeoutId=setTimeout(()=>this._origin=null,o)}})}_onFocus(t,n){let o=this._elementInfo.get(n),i=B(t);!o||!o.checkChildren&&n!==i||this._originChanged(n,this._getFocusOrigin(i),o)}_onBlur(t,n){let o=this._elementInfo.get(n);!o||o.checkChildren&&t.relatedTarget instanceof Node&&n.contains(t.relatedTarget)||(this._setClasses(n),this._emitOrigin(o,null))}_emitOrigin(t,n){t.subject.observers.length&&this._ngZone.run(()=>t.subject.next(n))}_registerGlobalListeners(t){if(!this._platform.isBrowser)return;let n=t.rootNode,o=this._rootNodeFocusListenerCount.get(n)||0;o||this._ngZone.runOutsideAngular(()=>{n.addEventListener("focus",this._rootNodeFocusAndBlurListener,Ye),n.addEventListener("blur",this._rootNodeFocusAndBlurListener,Ye)}),this._rootNodeFocusListenerCount.set(n,o+1),++this._monitoredElementCount===1&&(this._ngZone.runOutsideAngular(()=>{this._getWindow().addEventListener("focus",this._windowFocusListener)}),this._inputModalityDetector.modalityDetected.pipe(xt(this._stopInputModalityDetector)).subscribe(i=>{this._setOrigin(i,!0)}))}_removeGlobalListeners(t){let n=t.rootNode;if(this._rootNodeFocusListenerCount.has(n)){let o=this._rootNodeFocusListenerCount.get(n);o>1?this._rootNodeFocusListenerCount.set(n,o-1):(n.removeEventListener("focus",this._rootNodeFocusAndBlurListener,Ye),n.removeEventListener("blur",this._rootNodeFocusAndBlurListener,Ye),this._rootNodeFocusListenerCount.delete(n))}--this._monitoredElementCount||(this._getWindow().removeEventListener("focus",this._windowFocusListener),this._stopInputModalityDetector.next(),clearTimeout(this._windowFocusTimeoutId),clearTimeout(this._originTimeoutId))}_originChanged(t,n,o){this._setClasses(t,n),this._emitOrigin(o,n),this._lastFocusOrigin=n}_getClosestElementsInfo(t){let n=[];return this._elementInfo.forEach((o,i)=>{(i===t||o.checkChildren&&i.contains(t))&&n.push([i,o])}),n}_isLastInteractionFromInputLabel(t){let{_mostRecentTarget:n,mostRecentModality:o}=this._inputModalityDetector;if(o!=="mouse"||!n||n===t||t.nodeName!=="INPUT"&&t.nodeName!=="TEXTAREA"||t.disabled)return!1;let i=t.labels;if(i){for(let s=0;s<i.length;s++)if(i[s].contains(n))return!0}return!1}static \u0275fac=function(n){return new(n||e)};static \u0275prov=b({token:e,factory:e.\u0275fac,providedIn:"root"})}return e})();var Ko=new Set,yt,Gn=(()=>{class e{_platform=d($);_nonce=d(gr,{optional:!0});_matchMedia;constructor(){this._matchMedia=this._platform.isBrowser&&window.matchMedia?window.matchMedia.bind(window):os}matchMedia(t){return(this._platform.WEBKIT||this._platform.BLINK)&&rs(t,this._nonce),this._matchMedia(t)}static \u0275fac=function(n){return new(n||e)};static \u0275prov=b({token:e,factory:e.\u0275fac,providedIn:"root"})}return e})();function rs(e,r){if(!Ko.has(e))try{yt||(yt=document.createElement("style"),r&&yt.setAttribute("nonce",r),yt.setAttribute("type","text/css"),document.head.appendChild(yt)),yt.sheet&&(yt.sheet.insertRule(`@media ${e} {body{ }}`,0),Ko.add(e))}catch(t){console.error(t)}}function os(e){return{matches:e==="all"||e==="",media:e,addListener:()=>{},removeListener:()=>{}}}var is=new v("MATERIAL_ANIMATIONS"),Yo=null;function as(){return d(is,{optional:!0})?.animationsDisabled||d(ve,{optional:!0})==="NoopAnimations"?"di-disabled":(Yo??=d(Gn).matchMedia("(prefers-reduced-motion)").matches,Yo?"reduced-motion":"enabled")}function Xe(){return as()!=="enabled"}var F=(function(e){return e[e.FADING_IN=0]="FADING_IN",e[e.VISIBLE=1]="VISIBLE",e[e.FADING_OUT=2]="FADING_OUT",e[e.HIDDEN=3]="HIDDEN",e})(F||{}),Wn=class{_renderer;element;config;_animationForciblyDisabledThroughCss;state=F.HIDDEN;constructor(r,t,n,o=!1){this._renderer=r,this.element=t,this.config=n,this._animationForciblyDisabledThroughCss=o}fadeOut(){this._renderer.fadeOutRipple(this)}},Xo=zt({passive:!0,capture:!0}),Zn=class{_events=new Map;addHandler(r,t,n,o){let i=this._events.get(t);if(i){let s=i.get(n);s?s.add(o):i.set(n,new Set([o]))}else this._events.set(t,new Map([[n,new Set([o])]])),r.runOutsideAngular(()=>{document.addEventListener(t,this._delegateEventHandler,Xo)})}removeHandler(r,t,n){let o=this._events.get(r);if(!o)return;let i=o.get(t);i&&(i.delete(n),i.size===0&&o.delete(t),o.size===0&&(this._events.delete(r),document.removeEventListener(r,this._delegateEventHandler,Xo)))}_delegateEventHandler=r=>{let t=B(r);t&&this._events.get(r.type)?.forEach((n,o)=>{(o===t||o.contains(t))&&n.forEach(i=>i.handleEvent(r))})}},he={enterDuration:225,exitDuration:150},ss=800,Jo=zt({passive:!0,capture:!0}),ti=["mousedown","touchstart"],ei=["mouseup","mouseleave","touchend","touchcancel"],cs=(()=>{class e{static \u0275fac=function(n){return new(n||e)};static \u0275cmp=M({type:e,selectors:[["ng-component"]],hostAttrs:["mat-ripple-style-loader",""],decls:0,vars:0,template:function(n,o){},styles:[`.mat-ripple {
  overflow: hidden;
  position: relative;
}
.mat-ripple:not(:empty) {
  transform: translateZ(0);
}

.mat-ripple.mat-ripple-unbounded {
  overflow: visible;
}

.mat-ripple-element {
  position: absolute;
  border-radius: 50%;
  pointer-events: none;
  transition: opacity, transform 0ms cubic-bezier(0, 0, 0.2, 1);
  transform: scale3d(0, 0, 0);
  background-color: var(--mat-ripple-color, color-mix(in srgb, var(--mat-sys-on-surface) 10%, transparent));
}
@media (forced-colors: active) {
  .mat-ripple-element {
    display: none;
  }
}
.cdk-drag-preview .mat-ripple-element, .cdk-drag-placeholder .mat-ripple-element {
  display: none;
}
`],encapsulation:2,changeDetection:0})}return e})(),Je=class e{_target;_ngZone;_platform;_containerElement;_triggerElement=null;_isPointerDown=!1;_activeRipples=new Map;_mostRecentTransientRipple=null;_lastTouchStartEvent;_pointerUpEventsRegistered=!1;_containerRect=null;static _eventManager=new Zn;constructor(r,t,n,o,i){this._target=r,this._ngZone=t,this._platform=o,o.isBrowser&&(this._containerElement=st(n)),i&&i.get(xe).load(cs)}fadeInRipple(r,t,n={}){let o=this._containerRect=this._containerRect||this._containerElement.getBoundingClientRect(),i=m(m({},he),n.animation);n.centered&&(r=o.left+o.width/2,t=o.top+o.height/2);let s=n.radius||ls(r,t,o),a=r-o.left,c=t-o.top,l=i.enterDuration,u=document.createElement("div");u.classList.add("mat-ripple-element"),u.style.left=`${a-s}px`,u.style.top=`${c-s}px`,u.style.height=`${s*2}px`,u.style.width=`${s*2}px`,n.color!=null&&(u.style.backgroundColor=n.color),u.style.transitionDuration=`${l}ms`,this._containerElement.appendChild(u);let p=window.getComputedStyle(u),y=p.transitionProperty,H=p.transitionDuration,S=y==="none"||H==="0s"||H==="0s, 0s"||o.width===0&&o.height===0,x=new Wn(this,u,n,S);u.style.transform="scale3d(1, 1, 1)",x.state=F.FADING_IN,n.persistent||(this._mostRecentTransientRipple=x);let q=null;return!S&&(l||i.exitDuration)&&this._ngZone.runOutsideAngular(()=>{let _t=()=>{q&&(q.fallbackTimer=null),clearTimeout(ct),this._finishRippleTransition(x)},Z=()=>this._destroyRipple(x),ct=setTimeout(Z,l+100);u.addEventListener("transitionend",_t),u.addEventListener("transitioncancel",Z),q={onTransitionEnd:_t,onTransitionCancel:Z,fallbackTimer:ct}}),this._activeRipples.set(x,q),(S||!l)&&this._finishRippleTransition(x),x}fadeOutRipple(r){if(r.state===F.FADING_OUT||r.state===F.HIDDEN)return;let t=r.element,n=m(m({},he),r.config.animation);t.style.transitionDuration=`${n.exitDuration}ms`,t.style.opacity="0",r.state=F.FADING_OUT,(r._animationForciblyDisabledThroughCss||!n.exitDuration)&&this._finishRippleTransition(r)}fadeOutAll(){this._getActiveRipples().forEach(r=>r.fadeOut())}fadeOutAllNonPersistent(){this._getActiveRipples().forEach(r=>{r.config.persistent||r.fadeOut()})}setupTriggerEvents(r){let t=st(r);!this._platform.isBrowser||!t||t===this._triggerElement||(this._removeTriggerEvents(),this._triggerElement=t,ti.forEach(n=>{e._eventManager.addHandler(this._ngZone,n,t,this)}))}handleEvent(r){r.type==="mousedown"?this._onMousedown(r):r.type==="touchstart"?this._onTouchStart(r):this._onPointerUp(),this._pointerUpEventsRegistered||(this._ngZone.runOutsideAngular(()=>{ei.forEach(t=>{this._triggerElement.addEventListener(t,this,Jo)})}),this._pointerUpEventsRegistered=!0)}_finishRippleTransition(r){r.state===F.FADING_IN?this._startFadeOutTransition(r):r.state===F.FADING_OUT&&this._destroyRipple(r)}_startFadeOutTransition(r){let t=r===this._mostRecentTransientRipple,{persistent:n}=r.config;r.state=F.VISIBLE,!n&&(!t||!this._isPointerDown)&&r.fadeOut()}_destroyRipple(r){let t=this._activeRipples.get(r)??null;this._activeRipples.delete(r),this._activeRipples.size||(this._containerRect=null),r===this._mostRecentTransientRipple&&(this._mostRecentTransientRipple=null),r.state=F.HIDDEN,t!==null&&(r.element.removeEventListener("transitionend",t.onTransitionEnd),r.element.removeEventListener("transitioncancel",t.onTransitionCancel),t.fallbackTimer!==null&&clearTimeout(t.fallbackTimer)),r.element.remove()}_onMousedown(r){let t=de(r),n=this._lastTouchStartEvent&&Date.now()<this._lastTouchStartEvent+ss;!this._target.rippleDisabled&&!t&&!n&&(this._isPointerDown=!0,this.fadeInRipple(r.clientX,r.clientY,this._target.rippleConfig))}_onTouchStart(r){if(!this._target.rippleDisabled&&!ue(r)){this._lastTouchStartEvent=Date.now(),this._isPointerDown=!0;let t=r.changedTouches;if(t)for(let n=0;n<t.length;n++)this.fadeInRipple(t[n].clientX,t[n].clientY,this._target.rippleConfig)}}_onPointerUp(){this._isPointerDown&&(this._isPointerDown=!1,this._getActiveRipples().forEach(r=>{let t=r.state===F.VISIBLE||r.config.terminateOnPointerUp&&r.state===F.FADING_IN;!r.config.persistent&&t&&r.fadeOut()}))}_getActiveRipples(){return Array.from(this._activeRipples.keys())}_removeTriggerEvents(){let r=this._triggerElement;r&&(ti.forEach(t=>e._eventManager.removeHandler(t,r,this)),this._pointerUpEventsRegistered&&(ei.forEach(t=>r.removeEventListener(t,this,Jo)),this._pointerUpEventsRegistered=!1))}};function ls(e,r,t){let n=Math.max(Math.abs(e-t.left),Math.abs(e-t.right)),o=Math.max(Math.abs(r-t.top),Math.abs(r-t.bottom));return Math.sqrt(n*n+o*o)}var ni=new v("mat-ripple-global-options");var ds={capture:!0},us=["focus","mousedown","mouseenter","touchstart"],Qn="mat-ripple-loader-uninitialized",Kn="mat-ripple-loader-class-name",ri="mat-ripple-loader-centered",tn="mat-ripple-loader-disabled",oi=(()=>{class e{_document=d(G);_animationsDisabled=Xe();_globalRippleOptions=d(ni,{optional:!0});_platform=d($);_ngZone=d(k);_injector=d(dt);_eventCleanups;_hosts=new Map;constructor(){let t=d(It).createRenderer(null,null);this._eventCleanups=this._ngZone.runOutsideAngular(()=>us.map(n=>t.listen(this._document,n,this._onInteraction,ds)))}ngOnDestroy(){let t=this._hosts.keys();for(let n of t)this.destroyRipple(n);this._eventCleanups.forEach(n=>n())}configureRipple(t,n){t.setAttribute(Qn,this._globalRippleOptions?.namespace??""),(n.className||!t.hasAttribute(Kn))&&t.setAttribute(Kn,n.className||""),n.centered&&t.setAttribute(ri,""),n.disabled&&t.setAttribute(tn,"")}setDisabled(t,n){let o=this._hosts.get(t);o?(o.target.rippleDisabled=n,!n&&!o.hasSetUpEvents&&(o.hasSetUpEvents=!0,o.renderer.setupTriggerEvents(t))):n?t.setAttribute(tn,""):t.removeAttribute(tn)}_onInteraction=t=>{let n=B(t);if(n instanceof HTMLElement){let o=n.closest(`[${Qn}="${this._globalRippleOptions?.namespace??""}"]`);o&&this._createRipple(o)}};_createRipple(t){if(!this._document||this._hosts.has(t))return;t.querySelector(".mat-ripple")?.remove();let n=this._document.createElement("span");n.classList.add("mat-ripple",t.getAttribute(Kn)),t.append(n);let o=this._globalRippleOptions,i=this._animationsDisabled?0:o?.animation?.enterDuration??he.enterDuration,s=this._animationsDisabled?0:o?.animation?.exitDuration??he.exitDuration,a={rippleDisabled:this._animationsDisabled||o?.disabled||t.hasAttribute(tn),rippleConfig:{centered:t.hasAttribute(ri),terminateOnPointerUp:o?.terminateOnPointerUp,animation:{enterDuration:i,exitDuration:s}}},c=new Je(a,this._ngZone,n,this._platform,this._injector),l=!a.rippleDisabled;l&&c.setupTriggerEvents(t),this._hosts.set(t,{target:a,renderer:c,hasSetUpEvents:l}),t.removeAttribute(Qn)}destroyRipple(t){let n=this._hosts.get(t);n&&(n.renderer._removeTriggerEvents(),this._hosts.delete(t))}static \u0275fac=function(n){return new(n||e)};static \u0275prov=b({token:e,factory:e.\u0275fac,providedIn:"root"})}return e})();var ii=(()=>{class e{static \u0275fac=function(n){return new(n||e)};static \u0275cmp=M({type:e,selectors:[["structural-styles"]],decls:0,vars:0,template:function(n,o){},styles:[`.mat-focus-indicator {
  position: relative;
}
.mat-focus-indicator::before {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  box-sizing: border-box;
  pointer-events: none;
  display: var(--mat-focus-indicator-display, none);
  border-width: var(--mat-focus-indicator-border-width, 3px);
  border-style: var(--mat-focus-indicator-border-style, solid);
  border-color: var(--mat-focus-indicator-border-color, transparent);
  border-radius: var(--mat-focus-indicator-border-radius, 4px);
}
.mat-focus-indicator:focus-visible::before {
  content: "";
}

@media (forced-colors: active) {
  html {
    --mat-focus-indicator-display: block;
  }
}
`],encapsulation:2,changeDetection:0})}return e})();var ms=new v("MAT_BUTTON_CONFIG");function ai(e){return e==null?void 0:Ur(e)}var si=(()=>{class e{_elementRef=d(et);_ngZone=d(k);_animationsDisabled=Xe();_config=d(ms,{optional:!0});_focusMonitor=d(Vn);_cleanupClick;_renderer=d(vr);_rippleLoader=d(oi);_isAnchor;_isFab=!1;color;get disableRipple(){return this._disableRipple}set disableRipple(t){this._disableRipple=t,this._updateRippleDisabled()}_disableRipple=!1;get disabled(){return this._disabled}set disabled(t){this._disabled=t,this._updateRippleDisabled()}_disabled=!1;ariaDisabled;disabledInteractive;tabIndex;set _tabindex(t){this.tabIndex=t}constructor(){d(xe).load(ii);let t=this._elementRef.nativeElement;this._isAnchor=t.tagName==="A",this.disabledInteractive=this._config?.disabledInteractive??!1,this.color=this._config?.color??null,this._rippleLoader?.configureRipple(t,{className:"mat-mdc-button-ripple"})}ngAfterViewInit(){this._focusMonitor.monitor(this._elementRef,!0),this._isAnchor&&this._setupAsAnchor()}ngOnDestroy(){this._cleanupClick?.(),this._focusMonitor.stopMonitoring(this._elementRef),this._rippleLoader?.destroyRipple(this._elementRef.nativeElement)}focus(t="program",n){t?this._focusMonitor.focusVia(this._elementRef.nativeElement,t,n):this._elementRef.nativeElement.focus(n)}_getAriaDisabled(){return this.ariaDisabled!=null?this.ariaDisabled:this._isAnchor?this.disabled||null:this.disabled&&this.disabledInteractive?!0:null}_getDisabledAttribute(){return this.disabledInteractive||!this.disabled?null:!0}_updateRippleDisabled(){this._rippleLoader?.setDisabled(this._elementRef.nativeElement,this.disableRipple||this.disabled)}_getTabIndex(){return this._isAnchor?this.disabled&&!this.disabledInteractive?-1:this.tabIndex:this.tabIndex}_setupAsAnchor(){this._cleanupClick=this._ngZone.runOutsideAngular(()=>this._renderer.listen(this._elementRef.nativeElement,"click",t=>{this.disabled&&(t.preventDefault(),t.stopImmediatePropagation())}))}static \u0275fac=function(n){return new(n||e)};static \u0275dir=nt({type:e,hostAttrs:[1,"mat-mdc-button-base"],hostVars:13,hostBindings:function(n,o){n&2&&(Sr("disabled",o._getDisabledAttribute())("aria-disabled",o._getAriaDisabled())("tabindex",o._getTabIndex()),_e(o.color?"mat-"+o.color:""),mt("mat-mdc-button-disabled",o.disabled)("mat-mdc-button-disabled-interactive",o.disabledInteractive)("mat-unthemed",!o.color)("_mat-animation-noopable",o._animationsDisabled))},inputs:{color:"color",disableRipple:[2,"disableRipple","disableRipple",Et],disabled:[2,"disabled","disabled",Et],ariaDisabled:[2,"aria-disabled","ariaDisabled",Et],disabledInteractive:[2,"disabledInteractive","disabledInteractive",Et],tabIndex:[2,"tabIndex","tabIndex",ai],_tabindex:[2,"tabindex","_tabindex",ai]}})}return e})();var ci=(()=>{class e{static \u0275fac=function(n){return new(n||e)};static \u0275mod=St({type:e});static \u0275inj=Ct({imports:[Mt]})}return e})();var ps=["matButton",""],hs=[[["",8,"material-icons",3,"iconPositionEnd",""],["mat-icon",3,"iconPositionEnd",""],["","matButtonIcon","",3,"iconPositionEnd",""]],"*",[["","iconPositionEnd","",8,"material-icons"],["mat-icon","iconPositionEnd",""],["","matButtonIcon","","iconPositionEnd",""]]],fs=[".material-icons:not([iconPositionEnd]), mat-icon:not([iconPositionEnd]), [matButtonIcon]:not([iconPositionEnd])","*",".material-icons[iconPositionEnd], mat-icon[iconPositionEnd], [matButtonIcon][iconPositionEnd]"];var li=new Map([["text",["mat-mdc-button"]],["filled",["mdc-button--unelevated","mat-mdc-unelevated-button"]],["elevated",["mdc-button--raised","mat-mdc-raised-button"]],["outlined",["mdc-button--outlined","mat-mdc-outlined-button"]],["tonal",["mat-tonal-button"]]]),di=(()=>{class e extends si{get appearance(){return this._appearance}set appearance(t){this.setAppearance(t||this._config?.defaultAppearance||"text")}_appearance=null;constructor(){super();let t=gs(this._elementRef.nativeElement);t&&this.setAppearance(t)}setAppearance(t){if(t===this._appearance)return;let n=this._elementRef.nativeElement.classList,o=this._appearance?li.get(this._appearance):null,i=li.get(t);o&&n.remove(...o),n.add(...i),this._appearance=t}static \u0275fac=function(n){return new(n||e)};static \u0275cmp=M({type:e,selectors:[["button","matButton",""],["a","matButton",""],["button","mat-button",""],["button","mat-raised-button",""],["button","mat-flat-button",""],["button","mat-stroked-button",""],["a","mat-button",""],["a","mat-raised-button",""],["a","mat-flat-button",""],["a","mat-stroked-button",""]],hostAttrs:[1,"mdc-button"],inputs:{appearance:[0,"matButton","appearance"]},exportAs:["matButton","matAnchor"],features:[pn],attrs:ps,ngContentSelectors:fs,decls:7,vars:4,consts:[[1,"mat-mdc-button-persistent-ripple"],[1,"mdc-button__label"],[1,"mat-focus-indicator"],[1,"mat-mdc-button-touch-target"]],template:function(n,o){n&1&&(qt(hs),ye(0,"span",0),rt(1),Er(2,"span",1),rt(3,1),Mr(),rt(4,2),ye(5,"span",2)(6,"span",3)),n&2&&mt("mdc-button__ripple",!o._isFab)("mdc-fab__ripple",o._isFab)},styles:[`.mat-mdc-button-base {
  text-decoration: none;
}
.mat-mdc-button-base .mat-icon {
  min-height: fit-content;
  flex-shrink: 0;
}
@media (hover: none) {
  .mat-mdc-button-base:hover > span.mat-mdc-button-persistent-ripple::before {
    opacity: 0;
  }
}

.mdc-button {
  -webkit-user-select: none;
  user-select: none;
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  box-sizing: border-box;
  min-width: 64px;
  border: none;
  outline: none;
  line-height: inherit;
  -webkit-appearance: none;
  overflow: visible;
  vertical-align: middle;
  background: transparent;
  padding: 0 8px;
}
.mdc-button::-moz-focus-inner {
  padding: 0;
  border: 0;
}
.mdc-button:active {
  outline: none;
}
.mdc-button:hover {
  cursor: pointer;
}
.mdc-button:disabled {
  cursor: default;
  pointer-events: none;
}
.mdc-button[hidden] {
  display: none;
}
.mdc-button .mdc-button__label {
  position: relative;
}

.mat-mdc-button {
  padding: 0 var(--mat-button-text-horizontal-padding, 12px);
  height: var(--mat-button-text-container-height, 40px);
  font-family: var(--mat-button-text-label-text-font, var(--mat-sys-label-large-font));
  font-size: var(--mat-button-text-label-text-size, var(--mat-sys-label-large-size));
  letter-spacing: var(--mat-button-text-label-text-tracking, var(--mat-sys-label-large-tracking));
  text-transform: var(--mat-button-text-label-text-transform);
  font-weight: var(--mat-button-text-label-text-weight, var(--mat-sys-label-large-weight));
}
.mat-mdc-button, .mat-mdc-button .mdc-button__ripple {
  border-radius: var(--mat-button-text-container-shape, var(--mat-sys-corner-full));
}
.mat-mdc-button:not(:disabled) {
  color: var(--mat-button-text-label-text-color, var(--mat-sys-primary));
}
.mat-mdc-button[disabled], .mat-mdc-button.mat-mdc-button-disabled {
  cursor: default;
  pointer-events: none;
  color: var(--mat-button-text-disabled-label-text-color, color-mix(in srgb, var(--mat-sys-on-surface) 38%, transparent));
}
.mat-mdc-button.mat-mdc-button-disabled-interactive {
  pointer-events: auto;
}
.mat-mdc-button:has(.material-icons, mat-icon, [matButtonIcon]) {
  padding: 0 var(--mat-button-text-with-icon-horizontal-padding, 16px);
}
.mat-mdc-button > .mat-icon {
  margin-right: var(--mat-button-text-icon-spacing, 8px);
  margin-left: var(--mat-button-text-icon-offset, -4px);
}
[dir=rtl] .mat-mdc-button > .mat-icon {
  margin-right: var(--mat-button-text-icon-offset, -4px);
  margin-left: var(--mat-button-text-icon-spacing, 8px);
}
.mat-mdc-button .mdc-button__label + .mat-icon {
  margin-right: var(--mat-button-text-icon-offset, -4px);
  margin-left: var(--mat-button-text-icon-spacing, 8px);
}
[dir=rtl] .mat-mdc-button .mdc-button__label + .mat-icon {
  margin-right: var(--mat-button-text-icon-spacing, 8px);
  margin-left: var(--mat-button-text-icon-offset, -4px);
}
.mat-mdc-button .mat-ripple-element {
  background-color: var(--mat-button-text-ripple-color, color-mix(in srgb, var(--mat-sys-primary) calc(var(--mat-sys-pressed-state-layer-opacity) * 100%), transparent));
}
.mat-mdc-button .mat-mdc-button-persistent-ripple::before {
  background-color: var(--mat-button-text-state-layer-color, var(--mat-sys-primary));
}
.mat-mdc-button.mat-mdc-button-disabled .mat-mdc-button-persistent-ripple::before {
  background-color: var(--mat-button-text-disabled-state-layer-color, var(--mat-sys-on-surface-variant));
}
.mat-mdc-button:hover > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--mat-button-text-hover-state-layer-opacity, var(--mat-sys-hover-state-layer-opacity));
}
.mat-mdc-button.cdk-program-focused > .mat-mdc-button-persistent-ripple::before, .mat-mdc-button.cdk-keyboard-focused > .mat-mdc-button-persistent-ripple::before, .mat-mdc-button.mat-mdc-button-disabled-interactive:focus > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--mat-button-text-focus-state-layer-opacity, var(--mat-sys-focus-state-layer-opacity));
}
.mat-mdc-button:active > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--mat-button-text-pressed-state-layer-opacity, var(--mat-sys-pressed-state-layer-opacity));
}
.mat-mdc-button .mat-mdc-button-touch-target {
  position: absolute;
  top: 50%;
  height: var(--mat-button-text-touch-target-size, 48px);
  display: var(--mat-button-text-touch-target-display, block);
  left: 0;
  right: 0;
  transform: translateY(-50%);
}

.mat-mdc-unelevated-button {
  transition: box-shadow 280ms cubic-bezier(0.4, 0, 0.2, 1);
  height: var(--mat-button-filled-container-height, 40px);
  font-family: var(--mat-button-filled-label-text-font, var(--mat-sys-label-large-font));
  font-size: var(--mat-button-filled-label-text-size, var(--mat-sys-label-large-size));
  letter-spacing: var(--mat-button-filled-label-text-tracking, var(--mat-sys-label-large-tracking));
  text-transform: var(--mat-button-filled-label-text-transform);
  font-weight: var(--mat-button-filled-label-text-weight, var(--mat-sys-label-large-weight));
  padding: 0 var(--mat-button-filled-horizontal-padding, 24px);
}
.mat-mdc-unelevated-button > .mat-icon {
  margin-right: var(--mat-button-filled-icon-spacing, 8px);
  margin-left: var(--mat-button-filled-icon-offset, -8px);
}
[dir=rtl] .mat-mdc-unelevated-button > .mat-icon {
  margin-right: var(--mat-button-filled-icon-offset, -8px);
  margin-left: var(--mat-button-filled-icon-spacing, 8px);
}
.mat-mdc-unelevated-button .mdc-button__label + .mat-icon {
  margin-right: var(--mat-button-filled-icon-offset, -8px);
  margin-left: var(--mat-button-filled-icon-spacing, 8px);
}
[dir=rtl] .mat-mdc-unelevated-button .mdc-button__label + .mat-icon {
  margin-right: var(--mat-button-filled-icon-spacing, 8px);
  margin-left: var(--mat-button-filled-icon-offset, -8px);
}
.mat-mdc-unelevated-button .mat-ripple-element {
  background-color: var(--mat-button-filled-ripple-color, color-mix(in srgb, var(--mat-sys-on-primary) calc(var(--mat-sys-pressed-state-layer-opacity) * 100%), transparent));
}
.mat-mdc-unelevated-button .mat-mdc-button-persistent-ripple::before {
  background-color: var(--mat-button-filled-state-layer-color, var(--mat-sys-on-primary));
}
.mat-mdc-unelevated-button.mat-mdc-button-disabled .mat-mdc-button-persistent-ripple::before {
  background-color: var(--mat-button-filled-disabled-state-layer-color, var(--mat-sys-on-surface-variant));
}
.mat-mdc-unelevated-button:hover > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--mat-button-filled-hover-state-layer-opacity, var(--mat-sys-hover-state-layer-opacity));
}
.mat-mdc-unelevated-button.cdk-program-focused > .mat-mdc-button-persistent-ripple::before, .mat-mdc-unelevated-button.cdk-keyboard-focused > .mat-mdc-button-persistent-ripple::before, .mat-mdc-unelevated-button.mat-mdc-button-disabled-interactive:focus > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--mat-button-filled-focus-state-layer-opacity, var(--mat-sys-focus-state-layer-opacity));
}
.mat-mdc-unelevated-button:active > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--mat-button-filled-pressed-state-layer-opacity, var(--mat-sys-pressed-state-layer-opacity));
}
.mat-mdc-unelevated-button .mat-mdc-button-touch-target {
  position: absolute;
  top: 50%;
  height: var(--mat-button-filled-touch-target-size, 48px);
  display: var(--mat-button-filled-touch-target-display, block);
  left: 0;
  right: 0;
  transform: translateY(-50%);
}
.mat-mdc-unelevated-button:not(:disabled) {
  color: var(--mat-button-filled-label-text-color, var(--mat-sys-on-primary));
  background-color: var(--mat-button-filled-container-color, var(--mat-sys-primary));
}
.mat-mdc-unelevated-button, .mat-mdc-unelevated-button .mdc-button__ripple {
  border-radius: var(--mat-button-filled-container-shape, var(--mat-sys-corner-full));
}
.mat-mdc-unelevated-button[disabled], .mat-mdc-unelevated-button.mat-mdc-button-disabled {
  cursor: default;
  pointer-events: none;
  color: var(--mat-button-filled-disabled-label-text-color, color-mix(in srgb, var(--mat-sys-on-surface) 38%, transparent));
  background-color: var(--mat-button-filled-disabled-container-color, color-mix(in srgb, var(--mat-sys-on-surface) 12%, transparent));
}
.mat-mdc-unelevated-button.mat-mdc-button-disabled-interactive {
  pointer-events: auto;
}

.mat-mdc-raised-button {
  transition: box-shadow 280ms cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: var(--mat-button-protected-container-elevation-shadow, var(--mat-sys-level1));
  height: var(--mat-button-protected-container-height, 40px);
  font-family: var(--mat-button-protected-label-text-font, var(--mat-sys-label-large-font));
  font-size: var(--mat-button-protected-label-text-size, var(--mat-sys-label-large-size));
  letter-spacing: var(--mat-button-protected-label-text-tracking, var(--mat-sys-label-large-tracking));
  text-transform: var(--mat-button-protected-label-text-transform);
  font-weight: var(--mat-button-protected-label-text-weight, var(--mat-sys-label-large-weight));
  padding: 0 var(--mat-button-protected-horizontal-padding, 24px);
}
.mat-mdc-raised-button > .mat-icon {
  margin-right: var(--mat-button-protected-icon-spacing, 8px);
  margin-left: var(--mat-button-protected-icon-offset, -8px);
}
[dir=rtl] .mat-mdc-raised-button > .mat-icon {
  margin-right: var(--mat-button-protected-icon-offset, -8px);
  margin-left: var(--mat-button-protected-icon-spacing, 8px);
}
.mat-mdc-raised-button .mdc-button__label + .mat-icon {
  margin-right: var(--mat-button-protected-icon-offset, -8px);
  margin-left: var(--mat-button-protected-icon-spacing, 8px);
}
[dir=rtl] .mat-mdc-raised-button .mdc-button__label + .mat-icon {
  margin-right: var(--mat-button-protected-icon-spacing, 8px);
  margin-left: var(--mat-button-protected-icon-offset, -8px);
}
.mat-mdc-raised-button .mat-ripple-element {
  background-color: var(--mat-button-protected-ripple-color, color-mix(in srgb, var(--mat-sys-primary) calc(var(--mat-sys-pressed-state-layer-opacity) * 100%), transparent));
}
.mat-mdc-raised-button .mat-mdc-button-persistent-ripple::before {
  background-color: var(--mat-button-protected-state-layer-color, var(--mat-sys-primary));
}
.mat-mdc-raised-button.mat-mdc-button-disabled .mat-mdc-button-persistent-ripple::before {
  background-color: var(--mat-button-protected-disabled-state-layer-color, var(--mat-sys-on-surface-variant));
}
.mat-mdc-raised-button:hover > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--mat-button-protected-hover-state-layer-opacity, var(--mat-sys-hover-state-layer-opacity));
}
.mat-mdc-raised-button.cdk-program-focused > .mat-mdc-button-persistent-ripple::before, .mat-mdc-raised-button.cdk-keyboard-focused > .mat-mdc-button-persistent-ripple::before, .mat-mdc-raised-button.mat-mdc-button-disabled-interactive:focus > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--mat-button-protected-focus-state-layer-opacity, var(--mat-sys-focus-state-layer-opacity));
}
.mat-mdc-raised-button:active > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--mat-button-protected-pressed-state-layer-opacity, var(--mat-sys-pressed-state-layer-opacity));
}
.mat-mdc-raised-button .mat-mdc-button-touch-target {
  position: absolute;
  top: 50%;
  height: var(--mat-button-protected-touch-target-size, 48px);
  display: var(--mat-button-protected-touch-target-display, block);
  left: 0;
  right: 0;
  transform: translateY(-50%);
}
.mat-mdc-raised-button:not(:disabled) {
  color: var(--mat-button-protected-label-text-color, var(--mat-sys-primary));
  background-color: var(--mat-button-protected-container-color, var(--mat-sys-surface));
}
.mat-mdc-raised-button, .mat-mdc-raised-button .mdc-button__ripple {
  border-radius: var(--mat-button-protected-container-shape, var(--mat-sys-corner-full));
}
@media (hover: hover) {
  .mat-mdc-raised-button:hover {
    box-shadow: var(--mat-button-protected-hover-container-elevation-shadow, var(--mat-sys-level2));
  }
}
.mat-mdc-raised-button:focus {
  box-shadow: var(--mat-button-protected-focus-container-elevation-shadow, var(--mat-sys-level1));
}
.mat-mdc-raised-button:active, .mat-mdc-raised-button:focus:active {
  box-shadow: var(--mat-button-protected-pressed-container-elevation-shadow, var(--mat-sys-level1));
}
.mat-mdc-raised-button[disabled], .mat-mdc-raised-button.mat-mdc-button-disabled {
  cursor: default;
  pointer-events: none;
  color: var(--mat-button-protected-disabled-label-text-color, color-mix(in srgb, var(--mat-sys-on-surface) 38%, transparent));
  background-color: var(--mat-button-protected-disabled-container-color, color-mix(in srgb, var(--mat-sys-on-surface) 12%, transparent));
}
.mat-mdc-raised-button[disabled].mat-mdc-button-disabled, .mat-mdc-raised-button.mat-mdc-button-disabled.mat-mdc-button-disabled {
  box-shadow: var(--mat-button-protected-disabled-container-elevation-shadow, var(--mat-sys-level0));
}
.mat-mdc-raised-button.mat-mdc-button-disabled-interactive {
  pointer-events: auto;
}

.mat-mdc-outlined-button {
  border-style: solid;
  transition: border 280ms cubic-bezier(0.4, 0, 0.2, 1);
  height: var(--mat-button-outlined-container-height, 40px);
  font-family: var(--mat-button-outlined-label-text-font, var(--mat-sys-label-large-font));
  font-size: var(--mat-button-outlined-label-text-size, var(--mat-sys-label-large-size));
  letter-spacing: var(--mat-button-outlined-label-text-tracking, var(--mat-sys-label-large-tracking));
  text-transform: var(--mat-button-outlined-label-text-transform);
  font-weight: var(--mat-button-outlined-label-text-weight, var(--mat-sys-label-large-weight));
  border-radius: var(--mat-button-outlined-container-shape, var(--mat-sys-corner-full));
  border-width: var(--mat-button-outlined-outline-width, 1px);
  padding: 0 var(--mat-button-outlined-horizontal-padding, 24px);
}
.mat-mdc-outlined-button > .mat-icon {
  margin-right: var(--mat-button-outlined-icon-spacing, 8px);
  margin-left: var(--mat-button-outlined-icon-offset, -8px);
}
[dir=rtl] .mat-mdc-outlined-button > .mat-icon {
  margin-right: var(--mat-button-outlined-icon-offset, -8px);
  margin-left: var(--mat-button-outlined-icon-spacing, 8px);
}
.mat-mdc-outlined-button .mdc-button__label + .mat-icon {
  margin-right: var(--mat-button-outlined-icon-offset, -8px);
  margin-left: var(--mat-button-outlined-icon-spacing, 8px);
}
[dir=rtl] .mat-mdc-outlined-button .mdc-button__label + .mat-icon {
  margin-right: var(--mat-button-outlined-icon-spacing, 8px);
  margin-left: var(--mat-button-outlined-icon-offset, -8px);
}
.mat-mdc-outlined-button .mat-ripple-element {
  background-color: var(--mat-button-outlined-ripple-color, color-mix(in srgb, var(--mat-sys-primary) calc(var(--mat-sys-pressed-state-layer-opacity) * 100%), transparent));
}
.mat-mdc-outlined-button .mat-mdc-button-persistent-ripple::before {
  background-color: var(--mat-button-outlined-state-layer-color, var(--mat-sys-primary));
}
.mat-mdc-outlined-button.mat-mdc-button-disabled .mat-mdc-button-persistent-ripple::before {
  background-color: var(--mat-button-outlined-disabled-state-layer-color, var(--mat-sys-on-surface-variant));
}
.mat-mdc-outlined-button:hover > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--mat-button-outlined-hover-state-layer-opacity, var(--mat-sys-hover-state-layer-opacity));
}
.mat-mdc-outlined-button.cdk-program-focused > .mat-mdc-button-persistent-ripple::before, .mat-mdc-outlined-button.cdk-keyboard-focused > .mat-mdc-button-persistent-ripple::before, .mat-mdc-outlined-button.mat-mdc-button-disabled-interactive:focus > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--mat-button-outlined-focus-state-layer-opacity, var(--mat-sys-focus-state-layer-opacity));
}
.mat-mdc-outlined-button:active > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--mat-button-outlined-pressed-state-layer-opacity, var(--mat-sys-pressed-state-layer-opacity));
}
.mat-mdc-outlined-button .mat-mdc-button-touch-target {
  position: absolute;
  top: 50%;
  height: var(--mat-button-outlined-touch-target-size, 48px);
  display: var(--mat-button-outlined-touch-target-display, block);
  left: 0;
  right: 0;
  transform: translateY(-50%);
}
.mat-mdc-outlined-button:not(:disabled) {
  color: var(--mat-button-outlined-label-text-color, var(--mat-sys-primary));
  border-color: var(--mat-button-outlined-outline-color, var(--mat-sys-outline));
}
.mat-mdc-outlined-button[disabled], .mat-mdc-outlined-button.mat-mdc-button-disabled {
  cursor: default;
  pointer-events: none;
  color: var(--mat-button-outlined-disabled-label-text-color, color-mix(in srgb, var(--mat-sys-on-surface) 38%, transparent));
  border-color: var(--mat-button-outlined-disabled-outline-color, color-mix(in srgb, var(--mat-sys-on-surface) 12%, transparent));
}
.mat-mdc-outlined-button.mat-mdc-button-disabled-interactive {
  pointer-events: auto;
}

.mat-tonal-button {
  transition: box-shadow 280ms cubic-bezier(0.4, 0, 0.2, 1);
  height: var(--mat-button-tonal-container-height, 40px);
  font-family: var(--mat-button-tonal-label-text-font, var(--mat-sys-label-large-font));
  font-size: var(--mat-button-tonal-label-text-size, var(--mat-sys-label-large-size));
  letter-spacing: var(--mat-button-tonal-label-text-tracking, var(--mat-sys-label-large-tracking));
  text-transform: var(--mat-button-tonal-label-text-transform);
  font-weight: var(--mat-button-tonal-label-text-weight, var(--mat-sys-label-large-weight));
  padding: 0 var(--mat-button-tonal-horizontal-padding, 24px);
}
.mat-tonal-button:not(:disabled) {
  color: var(--mat-button-tonal-label-text-color, var(--mat-sys-on-secondary-container));
  background-color: var(--mat-button-tonal-container-color, var(--mat-sys-secondary-container));
}
.mat-tonal-button, .mat-tonal-button .mdc-button__ripple {
  border-radius: var(--mat-button-tonal-container-shape, var(--mat-sys-corner-full));
}
.mat-tonal-button[disabled], .mat-tonal-button.mat-mdc-button-disabled {
  cursor: default;
  pointer-events: none;
  color: var(--mat-button-tonal-disabled-label-text-color, color-mix(in srgb, var(--mat-sys-on-surface) 38%, transparent));
  background-color: var(--mat-button-tonal-disabled-container-color, color-mix(in srgb, var(--mat-sys-on-surface) 12%, transparent));
}
.mat-tonal-button.mat-mdc-button-disabled-interactive {
  pointer-events: auto;
}
.mat-tonal-button > .mat-icon {
  margin-right: var(--mat-button-tonal-icon-spacing, 8px);
  margin-left: var(--mat-button-tonal-icon-offset, -8px);
}
[dir=rtl] .mat-tonal-button > .mat-icon {
  margin-right: var(--mat-button-tonal-icon-offset, -8px);
  margin-left: var(--mat-button-tonal-icon-spacing, 8px);
}
.mat-tonal-button .mdc-button__label + .mat-icon {
  margin-right: var(--mat-button-tonal-icon-offset, -8px);
  margin-left: var(--mat-button-tonal-icon-spacing, 8px);
}
[dir=rtl] .mat-tonal-button .mdc-button__label + .mat-icon {
  margin-right: var(--mat-button-tonal-icon-spacing, 8px);
  margin-left: var(--mat-button-tonal-icon-offset, -8px);
}
.mat-tonal-button .mat-ripple-element {
  background-color: var(--mat-button-tonal-ripple-color, color-mix(in srgb, var(--mat-sys-on-secondary-container) calc(var(--mat-sys-pressed-state-layer-opacity) * 100%), transparent));
}
.mat-tonal-button .mat-mdc-button-persistent-ripple::before {
  background-color: var(--mat-button-tonal-state-layer-color, var(--mat-sys-on-secondary-container));
}
.mat-tonal-button.mat-mdc-button-disabled .mat-mdc-button-persistent-ripple::before {
  background-color: var(--mat-button-tonal-disabled-state-layer-color, var(--mat-sys-on-surface-variant));
}
.mat-tonal-button:hover > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--mat-button-tonal-hover-state-layer-opacity, var(--mat-sys-hover-state-layer-opacity));
}
.mat-tonal-button.cdk-program-focused > .mat-mdc-button-persistent-ripple::before, .mat-tonal-button.cdk-keyboard-focused > .mat-mdc-button-persistent-ripple::before, .mat-tonal-button.mat-mdc-button-disabled-interactive:focus > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--mat-button-tonal-focus-state-layer-opacity, var(--mat-sys-focus-state-layer-opacity));
}
.mat-tonal-button:active > .mat-mdc-button-persistent-ripple::before {
  opacity: var(--mat-button-tonal-pressed-state-layer-opacity, var(--mat-sys-pressed-state-layer-opacity));
}
.mat-tonal-button .mat-mdc-button-touch-target {
  position: absolute;
  top: 50%;
  height: var(--mat-button-tonal-touch-target-size, 48px);
  display: var(--mat-button-tonal-touch-target-display, block);
  left: 0;
  right: 0;
  transform: translateY(-50%);
}

.mat-mdc-button,
.mat-mdc-unelevated-button,
.mat-mdc-raised-button,
.mat-mdc-outlined-button,
.mat-tonal-button {
  -webkit-tap-highlight-color: transparent;
}
.mat-mdc-button .mat-mdc-button-ripple,
.mat-mdc-button .mat-mdc-button-persistent-ripple,
.mat-mdc-button .mat-mdc-button-persistent-ripple::before,
.mat-mdc-unelevated-button .mat-mdc-button-ripple,
.mat-mdc-unelevated-button .mat-mdc-button-persistent-ripple,
.mat-mdc-unelevated-button .mat-mdc-button-persistent-ripple::before,
.mat-mdc-raised-button .mat-mdc-button-ripple,
.mat-mdc-raised-button .mat-mdc-button-persistent-ripple,
.mat-mdc-raised-button .mat-mdc-button-persistent-ripple::before,
.mat-mdc-outlined-button .mat-mdc-button-ripple,
.mat-mdc-outlined-button .mat-mdc-button-persistent-ripple,
.mat-mdc-outlined-button .mat-mdc-button-persistent-ripple::before,
.mat-tonal-button .mat-mdc-button-ripple,
.mat-tonal-button .mat-mdc-button-persistent-ripple,
.mat-tonal-button .mat-mdc-button-persistent-ripple::before {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  pointer-events: none;
  border-radius: inherit;
}
.mat-mdc-button .mat-mdc-button-ripple,
.mat-mdc-unelevated-button .mat-mdc-button-ripple,
.mat-mdc-raised-button .mat-mdc-button-ripple,
.mat-mdc-outlined-button .mat-mdc-button-ripple,
.mat-tonal-button .mat-mdc-button-ripple {
  overflow: hidden;
}
.mat-mdc-button .mat-mdc-button-persistent-ripple::before,
.mat-mdc-unelevated-button .mat-mdc-button-persistent-ripple::before,
.mat-mdc-raised-button .mat-mdc-button-persistent-ripple::before,
.mat-mdc-outlined-button .mat-mdc-button-persistent-ripple::before,
.mat-tonal-button .mat-mdc-button-persistent-ripple::before {
  content: "";
  opacity: 0;
}
.mat-mdc-button .mdc-button__label,
.mat-mdc-button .mat-icon,
.mat-mdc-unelevated-button .mdc-button__label,
.mat-mdc-unelevated-button .mat-icon,
.mat-mdc-raised-button .mdc-button__label,
.mat-mdc-raised-button .mat-icon,
.mat-mdc-outlined-button .mdc-button__label,
.mat-mdc-outlined-button .mat-icon,
.mat-tonal-button .mdc-button__label,
.mat-tonal-button .mat-icon {
  z-index: 1;
  position: relative;
}
.mat-mdc-button .mat-focus-indicator,
.mat-mdc-unelevated-button .mat-focus-indicator,
.mat-mdc-raised-button .mat-focus-indicator,
.mat-mdc-outlined-button .mat-focus-indicator,
.mat-tonal-button .mat-focus-indicator {
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  position: absolute;
  border-radius: inherit;
}
.mat-mdc-button:focus-visible > .mat-focus-indicator::before,
.mat-mdc-unelevated-button:focus-visible > .mat-focus-indicator::before,
.mat-mdc-raised-button:focus-visible > .mat-focus-indicator::before,
.mat-mdc-outlined-button:focus-visible > .mat-focus-indicator::before,
.mat-tonal-button:focus-visible > .mat-focus-indicator::before {
  content: "";
  border-radius: inherit;
}
.mat-mdc-button._mat-animation-noopable,
.mat-mdc-unelevated-button._mat-animation-noopable,
.mat-mdc-raised-button._mat-animation-noopable,
.mat-mdc-outlined-button._mat-animation-noopable,
.mat-tonal-button._mat-animation-noopable {
  transition: none !important;
  animation: none !important;
}
.mat-mdc-button > .mat-icon,
.mat-mdc-unelevated-button > .mat-icon,
.mat-mdc-raised-button > .mat-icon,
.mat-mdc-outlined-button > .mat-icon,
.mat-tonal-button > .mat-icon {
  display: inline-block;
  position: relative;
  vertical-align: top;
  font-size: 1.125rem;
  height: 1.125rem;
  width: 1.125rem;
}

.mat-mdc-outlined-button .mat-mdc-button-ripple,
.mat-mdc-outlined-button .mdc-button__ripple {
  top: -1px;
  left: -1px;
  bottom: -1px;
  right: -1px;
}

.mat-mdc-unelevated-button .mat-focus-indicator::before,
.mat-tonal-button .mat-focus-indicator::before,
.mat-mdc-raised-button .mat-focus-indicator::before {
  margin: calc(calc(var(--mat-focus-indicator-border-width, 3px) + 2px) * -1);
}

.mat-mdc-outlined-button .mat-focus-indicator::before {
  margin: calc(calc(var(--mat-focus-indicator-border-width, 3px) + 3px) * -1);
}
`,`@media (forced-colors: active) {
  .mat-mdc-button:not(.mdc-button--outlined),
  .mat-mdc-unelevated-button:not(.mdc-button--outlined),
  .mat-mdc-raised-button:not(.mdc-button--outlined),
  .mat-mdc-outlined-button:not(.mdc-button--outlined),
  .mat-mdc-button-base.mat-tonal-button,
  .mat-mdc-icon-button.mat-mdc-icon-button,
  .mat-mdc-outlined-button .mdc-button__ripple {
    outline: solid 1px;
  }
}
`],encapsulation:2,changeDetection:0})}return e})();function gs(e){return e.hasAttribute("mat-raised-button")?"elevated":e.hasAttribute("mat-stroked-button")?"outlined":e.hasAttribute("mat-flat-button")?"filled":e.hasAttribute("mat-button")?"text":null}var ui=(()=>{class e{static \u0275fac=function(n){return new(n||e)};static \u0275mod=St({type:e});static \u0275inj=Ct({imports:[ci,Mt]})}return e})();var vs=["*",[["mat-toolbar-row"]]],ys=["*","mat-toolbar-row"],_s=(()=>{class e{static \u0275fac=function(n){return new(n||e)};static \u0275dir=nt({type:e,selectors:[["mat-toolbar-row"]],hostAttrs:[1,"mat-toolbar-row"],exportAs:["matToolbarRow"]})}return e})(),mi=(()=>{class e{_elementRef=d(et);_platform=d($);_document=d(G);color;_toolbarRows;constructor(){}ngAfterViewInit(){this._platform.isBrowser&&(this._checkToolbarMixedModes(),this._toolbarRows.changes.subscribe(()=>this._checkToolbarMixedModes()))}_checkToolbarMixedModes(){this._toolbarRows.length}static \u0275fac=function(n){return new(n||e)};static \u0275cmp=M({type:e,selectors:[["mat-toolbar"]],contentQueries:function(n,o,i){if(n&1&&Ar(i,_s,5),n&2){let s;Tr(s=Dr())&&(o._toolbarRows=s)}},hostAttrs:[1,"mat-toolbar"],hostVars:6,hostBindings:function(n,o){n&2&&(_e(o.color?"mat-"+o.color:""),mt("mat-toolbar-multiple-rows",o._toolbarRows.length>0)("mat-toolbar-single-row",o._toolbarRows.length===0))},inputs:{color:"color"},exportAs:["matToolbar"],ngContentSelectors:ys,decls:2,vars:0,template:function(n,o){n&1&&(qt(vs),rt(0),rt(1,1))},styles:[`.mat-toolbar {
  background: var(--mat-toolbar-container-background-color, var(--mat-sys-surface));
  color: var(--mat-toolbar-container-text-color, var(--mat-sys-on-surface));
}
.mat-toolbar, .mat-toolbar h1, .mat-toolbar h2, .mat-toolbar h3, .mat-toolbar h4, .mat-toolbar h5, .mat-toolbar h6 {
  font-family: var(--mat-toolbar-title-text-font, var(--mat-sys-title-large-font));
  font-size: var(--mat-toolbar-title-text-size, var(--mat-sys-title-large-size));
  line-height: var(--mat-toolbar-title-text-line-height, var(--mat-sys-title-large-line-height));
  font-weight: var(--mat-toolbar-title-text-weight, var(--mat-sys-title-large-weight));
  letter-spacing: var(--mat-toolbar-title-text-tracking, var(--mat-sys-title-large-tracking));
  margin: 0;
}
@media (forced-colors: active) {
  .mat-toolbar {
    outline: solid 1px;
  }
}
.mat-toolbar .mat-form-field-underline,
.mat-toolbar .mat-form-field-ripple,
.mat-toolbar .mat-focused .mat-form-field-ripple {
  background-color: currentColor;
}
.mat-toolbar .mat-form-field-label,
.mat-toolbar .mat-focused .mat-form-field-label,
.mat-toolbar .mat-select-value,
.mat-toolbar .mat-select-arrow,
.mat-toolbar .mat-form-field.mat-focused .mat-select-arrow {
  color: inherit;
}
.mat-toolbar .mat-input-element {
  caret-color: currentColor;
}
.mat-toolbar .mat-mdc-button-base.mat-mdc-button-base.mat-unthemed {
  --mat-button-text-label-text-color: var(--mat-toolbar-container-text-color, var(--mat-sys-on-surface));
  --mat-button-outlined-label-text-color: var(--mat-toolbar-container-text-color, var(--mat-sys-on-surface));
}

.mat-toolbar-row, .mat-toolbar-single-row {
  display: flex;
  box-sizing: border-box;
  padding: 0 16px;
  width: 100%;
  flex-direction: row;
  align-items: center;
  white-space: nowrap;
  height: var(--mat-toolbar-standard-height, 64px);
}
@media (max-width: 599px) {
  .mat-toolbar-row, .mat-toolbar-single-row {
    height: var(--mat-toolbar-mobile-height, 56px);
  }
}

.mat-toolbar-multiple-rows {
  display: flex;
  box-sizing: border-box;
  flex-direction: column;
  width: 100%;
  min-height: var(--mat-toolbar-standard-height, 64px);
}
@media (max-width: 599px) {
  .mat-toolbar-multiple-rows {
    min-height: var(--mat-toolbar-mobile-height, 56px);
  }
}
`],encapsulation:2,changeDetection:0})}return e})();var pi=(()=>{class e{static \u0275fac=function(n){return new(n||e)};static \u0275mod=St({type:e});static \u0275inj=Ct({imports:[Mt]})}return e})();var en=class e{title="OpsSphere";static \u0275fac=function(t){return new(t||e)};static \u0275cmp=M({type:e,selectors:[["app-root"]],decls:11,vars:1,consts:[[1,"app-toolbar"],[1,"brand"],["aria-hidden","true"],[1,"toolbar-spacer"],["mat-stroked-button","","type","button","disabled",""],[1,"app-shell"]],template:function(t,n){t&1&&(Bt(0,"mat-toolbar",0)(1,"div",1)(2,"mat-icon",2),we(3,"hub"),$t(),Bt(4,"span"),we(5),$t()(),Ht(6,"span",3),Bt(7,"button",4),we(8,"Frontend Foundation"),$t()(),Bt(9,"main",5),Ht(10,"router-outlet"),$t()),t&2&&(br(5),Nr(n.title))},dependencies:[ae,ui,di,Vr,qr,pi,mi],styles:["[_nghost-%COMP%]{display:block;min-height:100%}.app-toolbar[_ngcontent-%COMP%]{background:#fff;border-bottom:1px solid #d8dee8;color:#172033}.brand[_ngcontent-%COMP%]{display:inline-flex;align-items:center;gap:.5rem;font-weight:600}.toolbar-spacer[_ngcontent-%COMP%]{flex:1 1 auto}.app-shell[_ngcontent-%COMP%]{width:min(1120px,100% - 2rem);margin:0 auto;padding:2rem 0}"],changeDetection:0})};var Rs="@",xs=(()=>{class e{doc;delegate;zone;animationType;moduleImpl;_rendererFactoryPromise=null;scheduler=null;injector=d(dt);loadingSchedulerFn=d(Cs,{optional:!0});_engine;constructor(t,n,o,i,s){this.doc=t,this.delegate=n,this.zone=o,this.animationType=i,this.moduleImpl=s}ngOnDestroy(){this._engine?.flush()}loadImpl(){let t=()=>this.moduleImpl??import("./chunk-LLIUFC3H.js").then(o=>o),n;return this.loadingSchedulerFn?n=this.loadingSchedulerFn(t):n=t(),n.catch(o=>{throw new _(5300,!1)}).then(({\u0275createEngine:o,\u0275AnimationRendererFactory:i})=>{this._engine=o(this.animationType,this.doc);let s=new i(this.delegate,this._engine,this.zone);return this.delegate=s,s})}createRenderer(t,n){let o=this.delegate.createRenderer(t,n);if(o.\u0275type===0)return o;typeof o.throwOnSyntheticProps=="boolean"&&(o.throwOnSyntheticProps=!1);let i=new Yn(o);return n?.data?.animation&&!this._rendererFactoryPromise&&(this._rendererFactoryPromise=this.loadImpl()),this._rendererFactoryPromise?.then(s=>{let a=s.createRenderer(t,n);i.use(a),this.scheduler??=this.injector.get(pr,null,{optional:!0}),this.scheduler?.notify(10)}).catch(s=>{i.use(o)}),i}begin(){this.delegate.begin?.()}end(){this.delegate.end?.()}whenRenderingDone(){return this.delegate.whenRenderingDone?.()??Promise.resolve()}componentReplaced(t){this._engine?.flush(),this.delegate.componentReplaced?.(t)}static \u0275fac=function(n){yr()};static \u0275prov=b({token:e,factory:e.\u0275fac})}return e})(),Yn=class{delegate;replay=[];\u0275type=1;constructor(r){this.delegate=r}use(r){if(this.delegate=r,this.replay!==null){for(let t of this.replay)t(r);this.replay=null}}get data(){return this.delegate.data}destroy(){this.replay=null,this.delegate.destroy()}createElement(r,t){return this.delegate.createElement(r,t)}createComment(r){return this.delegate.createComment(r)}createText(r){return this.delegate.createText(r)}get destroyNode(){return this.delegate.destroyNode}appendChild(r,t){this.delegate.appendChild(r,t)}insertBefore(r,t,n,o){this.delegate.insertBefore(r,t,n,o)}removeChild(r,t,n,o){this.delegate.removeChild(r,t,n,o)}selectRootElement(r,t){return this.delegate.selectRootElement(r,t)}parentNode(r){return this.delegate.parentNode(r)}nextSibling(r){return this.delegate.nextSibling(r)}setAttribute(r,t,n,o){this.delegate.setAttribute(r,t,n,o)}removeAttribute(r,t,n){this.delegate.removeAttribute(r,t,n)}addClass(r,t){this.delegate.addClass(r,t)}removeClass(r,t){this.delegate.removeClass(r,t)}setStyle(r,t,n,o){this.delegate.setStyle(r,t,n,o)}removeStyle(r,t,n){this.delegate.removeStyle(r,t,n)}setProperty(r,t,n){this.shouldReplay(t)&&this.replay.push(o=>o.setProperty(r,t,n)),this.delegate.setProperty(r,t,n)}setValue(r,t){this.delegate.setValue(r,t)}listen(r,t,n,o){return this.shouldReplay(t)&&this.replay.push(i=>i.listen(r,t,n,o)),this.delegate.listen(r,t,n,o)}shouldReplay(r){return this.replay!==null&&r.startsWith(Rs)}},Cs=new v("");function hi(e="animations"){return un("NgAsyncAnimations"),ge([{provide:It,useFactory:()=>new xs(d(G),d(jr),d(k),e)},{provide:ve,useValue:e==="noop"?"NoopAnimations":"BrowserAnimations"}])}var fi=()=>!0;var gi=[{path:"",canActivate:[fi],loadComponent:()=>import("./chunk-JND7EIOO.js").then(e=>e.HomePlaceholderComponent)},{path:"**",redirectTo:""}];var bi=(e,r)=>r(e);var vi={providers:[mr(),kr(),zn(gi),Br($r([bi])),hi()]};zr(en,vi).catch(e=>console.error(e));
