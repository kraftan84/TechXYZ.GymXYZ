/* @ds-bundle: {"format":3,"namespace":"TechXYZDesignSystem_ff9a8f","components":[{"name":"Button","sourcePath":"components/buttons/Button.jsx"},{"name":"IconButton","sourcePath":"components/buttons/IconButton.jsx"},{"name":"Avatar","sourcePath":"components/display/Avatar.jsx"},{"name":"Badge","sourcePath":"components/display/Badge.jsx"},{"name":"Card","sourcePath":"components/display/Card.jsx"},{"name":"Checkbox","sourcePath":"components/forms/Checkbox.jsx"},{"name":"Input","sourcePath":"components/forms/Input.jsx"},{"name":"Select","sourcePath":"components/forms/Select.jsx"},{"name":"Switch","sourcePath":"components/forms/Switch.jsx"}],"sourceHashes":{"components/buttons/Button.jsx":"5bbdb4f6e8af","components/buttons/IconButton.jsx":"9a9bdcb0ab15","components/display/Avatar.jsx":"2f0cabedeba6","components/display/Badge.jsx":"bbc6ff5300d9","components/display/Card.jsx":"1be093378c77","components/forms/Checkbox.jsx":"a4eb10189039","components/forms/Input.jsx":"66a76f9af86f","components/forms/Select.jsx":"53f060e9144b","components/forms/Switch.jsx":"a05899f38b7d","ui_kits/app/Dashboard.jsx":"0f034c8d593e","ui_kits/app/LoginScreen.jsx":"e6dc7eeed377","ui_kits/app/RequestDetail.jsx":"c8e3a7365be6","ui_kits/app/RequestsList.jsx":"edc5149da375","ui_kits/app/Sidebar.jsx":"0c42efaf1b2a","ui_kits/app/Topbar.jsx":"ab6efd9d4df8","ui_kits/vitrine/Audiences.jsx":"d48964169650","ui_kits/vitrine/ContactCTA.jsx":"3b5aba6141fc","ui_kits/vitrine/Footer.jsx":"3e4c4b801ff4","ui_kits/vitrine/Hero.jsx":"c23113739a08","ui_kits/vitrine/NavBar.jsx":"391f9cd48d05","ui_kits/vitrine/Process.jsx":"454692d31872","ui_kits/vitrine/Services.jsx":"6a3b005a7bf8"},"inlinedExternals":[],"unexposedExports":[]} */

(() => {

const __ds_ns = (window.TechXYZDesignSystem_ff9a8f = window.TechXYZDesignSystem_ff9a8f || {});

const __ds_scope = {};

(__ds_ns.__errors = __ds_ns.__errors || []);

// components/buttons/Button.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/**
 * TechXYZ Button — the primary action primitive.
 * Variants: primary (azure spark), secondary (ink), ghost, outline, danger.
 * All styling is driven by design-system CSS custom properties.
 */
function Button({
  children,
  variant = "primary",
  size = "md",
  iconLeft = null,
  iconRight = null,
  loading = false,
  disabled = false,
  type = "button",
  onClick,
  style,
  ...rest
}) {
  const sizes = {
    sm: {
      height: "var(--control-sm)",
      padding: "0 14px",
      fontSize: "var(--text-sm)",
      gap: "6px"
    },
    md: {
      height: "var(--control-md)",
      padding: "0 20px",
      fontSize: "var(--text-md)",
      gap: "8px"
    },
    lg: {
      height: "var(--control-lg)",
      padding: "0 28px",
      fontSize: "var(--text-lg)",
      gap: "10px"
    }
  };
  const variants = {
    primary: {
      background: "var(--color-primary)",
      color: "var(--color-on-primary)",
      border: "1.5px solid transparent",
      boxShadow: "var(--shadow-brand)"
    },
    secondary: {
      background: "var(--color-secondary)",
      color: "var(--color-on-secondary)",
      border: "1.5px solid transparent",
      boxShadow: "var(--shadow-sm)"
    },
    outline: {
      background: "var(--surface-card)",
      color: "var(--text-strong)",
      border: "1.5px solid var(--border-default)",
      boxShadow: "var(--shadow-xs)"
    },
    ghost: {
      background: "transparent",
      color: "var(--text-strong)",
      border: "1.5px solid transparent",
      boxShadow: "none"
    },
    danger: {
      background: "var(--color-danger)",
      color: "#fff",
      border: "1.5px solid transparent",
      boxShadow: "var(--shadow-sm)"
    }
  };
  const isDisabled = disabled || loading;
  return /*#__PURE__*/React.createElement("button", _extends({
    type: type,
    onClick: onClick,
    disabled: isDisabled,
    className: "txyz-btn",
    "data-variant": variant,
    style: {
      display: "inline-flex",
      alignItems: "center",
      justifyContent: "center",
      gap: sizes[size].gap,
      height: sizes[size].height,
      padding: sizes[size].padding,
      fontFamily: "var(--font-sans)",
      fontWeight: "var(--weight-semibold)",
      fontSize: sizes[size].fontSize,
      lineHeight: 1,
      letterSpacing: "0.01em",
      borderRadius: "var(--radius-md)",
      cursor: isDisabled ? "not-allowed" : "pointer",
      opacity: isDisabled ? 0.55 : 1,
      transition: "transform var(--duration-fast) var(--ease-standard), background var(--duration-fast) var(--ease-standard), box-shadow var(--duration-fast) var(--ease-standard)",
      whiteSpace: "nowrap",
      userSelect: "none",
      ...variants[variant],
      ...style
    }
  }, rest), /*#__PURE__*/React.createElement("style", null, `
        .txyz-btn:not(:disabled):hover[data-variant="primary"]{background:var(--color-primary-hover)!important}
        .txyz-btn:not(:disabled):active[data-variant="primary"]{background:var(--color-primary-active)!important;transform:scale(.98)}
        .txyz-btn:not(:disabled):hover[data-variant="secondary"]{background:var(--color-secondary-hover)!important}
        .txyz-btn:not(:disabled):active[data-variant="secondary"]{background:var(--color-secondary-active)!important;transform:scale(.98)}
        .txyz-btn:not(:disabled):hover[data-variant="outline"]{background:var(--surface-sunken)!important;border-color:var(--border-strong)!important}
        .txyz-btn:not(:disabled):active[data-variant="outline"]{transform:scale(.98)}
        .txyz-btn:not(:disabled):hover[data-variant="ghost"]{background:var(--surface-sunken)!important}
        .txyz-btn:not(:disabled):active[data-variant="ghost"]{transform:scale(.98)}
        .txyz-btn:not(:disabled):hover[data-variant="danger"]{background:var(--danger-600)!important}
        .txyz-btn:not(:disabled):active[data-variant="danger"]{background:var(--danger-700)!important;transform:scale(.98)}
        .txyz-btn:focus-visible{outline:none;box-shadow:var(--ring-brand)}
      `), loading && /*#__PURE__*/React.createElement("span", {
    "aria-hidden": "true",
    style: {
      width: "1em",
      height: "1em",
      borderRadius: "50%",
      border: "2px solid currentColor",
      borderTopColor: "transparent",
      display: "inline-block",
      animation: "txyz-spin 0.7s linear infinite"
    }
  }), !loading && iconLeft, children, !loading && iconRight, /*#__PURE__*/React.createElement("style", null, `@keyframes txyz-spin{to{transform:rotate(360deg)}}`));
}
Object.assign(__ds_scope, { Button });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/buttons/Button.jsx", error: String((e && e.message) || e) }); }

// components/buttons/IconButton.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/**
 * TechXYZ IconButton — a square, icon-only control for toolbars and
 * compact actions. Pass a Lucide <i> or SVG as children.
 */
function IconButton({
  children,
  variant = "ghost",
  size = "md",
  label,
  disabled = false,
  onClick,
  style,
  ...rest
}) {
  const dims = {
    sm: 32,
    md: 40,
    lg: 48
  };
  const d = dims[size];
  const variants = {
    primary: {
      background: "var(--color-primary)",
      color: "#fff",
      border: "1.5px solid transparent",
      boxShadow: "var(--shadow-brand)"
    },
    outline: {
      background: "var(--surface-card)",
      color: "var(--text-strong)",
      border: "1.5px solid var(--border-default)"
    },
    ghost: {
      background: "transparent",
      color: "var(--text-muted)",
      border: "1.5px solid transparent"
    }
  };
  return /*#__PURE__*/React.createElement("button", _extends({
    type: "button",
    "aria-label": label,
    title: label,
    onClick: onClick,
    disabled: disabled,
    className: "txyz-iconbtn",
    "data-variant": variant,
    style: {
      width: d,
      height: d,
      display: "inline-flex",
      alignItems: "center",
      justifyContent: "center",
      borderRadius: "var(--radius-md)",
      cursor: disabled ? "not-allowed" : "pointer",
      opacity: disabled ? 0.5 : 1,
      transition: "transform var(--duration-fast) var(--ease-standard), background var(--duration-fast) var(--ease-standard)",
      ...variants[variant],
      ...style
    }
  }, rest), /*#__PURE__*/React.createElement("style", null, `
        .txyz-iconbtn:not(:disabled):hover[data-variant="ghost"]{background:var(--surface-sunken)!important;color:var(--text-strong)!important}
        .txyz-iconbtn:not(:disabled):hover[data-variant="outline"]{background:var(--surface-sunken)!important}
        .txyz-iconbtn:not(:disabled):hover[data-variant="primary"]{background:var(--color-primary-hover)!important}
        .txyz-iconbtn:not(:disabled):active{transform:scale(.92)}
        .txyz-iconbtn:focus-visible{outline:none;box-shadow:var(--ring-brand)}
      `), children);
}
Object.assign(__ds_scope, { IconButton });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/buttons/IconButton.jsx", error: String((e && e.message) || e) }); }

// components/display/Avatar.jsx
try { (() => {
/**
 * TechXYZ Avatar — circular user/org token. Shows an image, or initials on a
 * brand-tinted background as a fallback.
 */
function Avatar({
  src,
  name = "",
  size = "md",
  style
}) {
  const dims = {
    xs: 24,
    sm: 32,
    md: 40,
    lg: 56,
    xl: 72
  };
  const d = dims[size] || dims.md;
  const initials = name.split(" ").filter(Boolean).slice(0, 2).map(w => w[0]).join("").toUpperCase();
  return /*#__PURE__*/React.createElement("span", {
    style: {
      width: d,
      height: d,
      flex: "none",
      borderRadius: "50%",
      overflow: "hidden",
      display: "inline-flex",
      alignItems: "center",
      justifyContent: "center",
      background: "var(--azure-50)",
      color: "var(--azure-700)",
      border: "1.5px solid var(--azure-100)",
      fontFamily: "var(--font-sans)",
      fontWeight: "var(--weight-bold)",
      fontSize: d * 0.4,
      letterSpacing: "0.01em",
      userSelect: "none",
      ...style
    }
  }, src ? /*#__PURE__*/React.createElement("img", {
    src: src,
    alt: name,
    style: {
      width: "100%",
      height: "100%",
      objectFit: "cover"
    }
  }) : initials || "?");
}
Object.assign(__ds_scope, { Avatar });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/display/Avatar.jsx", error: String((e && e.message) || e) }); }

// components/display/Badge.jsx
try { (() => {
/**
 * TechXYZ Badge — a small status/label pill.
 * Tones map to the semantic palette; `soft` (default) uses tinted backgrounds,
 * `solid` uses filled color.
 */
function Badge({
  children,
  tone = "neutral",
  variant = "soft",
  dot = false,
  iconLeft = null,
  style
}) {
  const tones = {
    neutral: {
      soft: ["var(--surface-sunken)", "var(--neutral-700)"],
      solid: ["var(--neutral-700)", "#fff"],
      dot: "var(--neutral-500)"
    },
    brand: {
      soft: ["var(--azure-50)", "var(--azure-700)"],
      solid: ["var(--color-primary)", "#fff"],
      dot: "var(--azure-500)"
    },
    success: {
      soft: ["var(--success-50)", "var(--success-700)"],
      solid: ["var(--color-success)", "#fff"],
      dot: "var(--success-500)"
    },
    warning: {
      soft: ["var(--warning-50)", "var(--warning-700)"],
      solid: ["var(--color-warning)", "#fff"],
      dot: "var(--warning-500)"
    },
    danger: {
      soft: ["var(--danger-50)", "var(--danger-700)"],
      solid: ["var(--color-danger)", "#fff"],
      dot: "var(--danger-500)"
    }
  };
  const [bg, fg] = tones[tone][variant === "solid" ? "solid" : "soft"];
  return /*#__PURE__*/React.createElement("span", {
    style: {
      display: "inline-flex",
      alignItems: "center",
      gap: "6px",
      height: "24px",
      padding: "0 10px",
      background: bg,
      color: fg,
      fontFamily: "var(--font-sans)",
      fontSize: "var(--text-xs)",
      fontWeight: "var(--weight-semibold)",
      letterSpacing: "0.01em",
      borderRadius: "var(--radius-pill)",
      whiteSpace: "nowrap",
      ...style
    }
  }, dot && /*#__PURE__*/React.createElement("span", {
    "aria-hidden": "true",
    style: {
      width: 7,
      height: 7,
      borderRadius: "50%",
      background: variant === "solid" ? "currentColor" : tones[tone].dot,
      flex: "none"
    }
  }), iconLeft, children);
}
Object.assign(__ds_scope, { Badge });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/display/Badge.jsx", error: String((e && e.message) || e) }); }

// components/display/Card.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/**
 * TechXYZ Card — the surface primitive. White, lightly rounded, hairline
 * border, soft shadow. `interactive` adds hover lift; `accent` adds an azure
 * top bar for featured cards.
 */
function Card({
  children,
  interactive = false,
  accent = false,
  padding = "var(--space-6)",
  as = "div",
  style,
  ...rest
}) {
  const Tag = as;
  return /*#__PURE__*/React.createElement(Tag, _extends({
    className: interactive ? "txyz-card txyz-card--int" : "txyz-card",
    style: {
      position: "relative",
      background: "var(--surface-card)",
      border: "1px solid var(--border-subtle)",
      borderRadius: "var(--radius-lg)",
      boxShadow: "var(--shadow-sm)",
      padding,
      overflow: "hidden",
      transition: "transform var(--duration-base) var(--ease-standard), box-shadow var(--duration-base) var(--ease-standard)",
      ...style
    }
  }, rest), accent && /*#__PURE__*/React.createElement("span", {
    "aria-hidden": "true",
    style: {
      position: "absolute",
      insetInline: 0,
      top: 0,
      height: 3,
      background: "var(--gradient-spark)"
    }
  }), /*#__PURE__*/React.createElement("style", null, `
        .txyz-card--int{cursor:pointer}
        .txyz-card--int:hover{transform:translateY(-2px);box-shadow:var(--shadow-md)}
      `), children);
}
Object.assign(__ds_scope, { Card });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/display/Card.jsx", error: String((e && e.message) || e) }); }

// components/forms/Checkbox.jsx
try { (() => {
/**
 * TechXYZ Checkbox — a labeled checkbox with brand-colored checked state.
 */
function Checkbox({
  checked = false,
  onChange,
  label,
  disabled = false,
  id,
  style
}) {
  const handle = () => {
    if (!disabled && onChange) onChange(!checked);
  };
  return /*#__PURE__*/React.createElement("label", {
    style: {
      display: "inline-flex",
      alignItems: "flex-start",
      gap: "10px",
      cursor: disabled ? "not-allowed" : "pointer",
      opacity: disabled ? 0.55 : 1,
      fontFamily: "var(--font-sans)",
      fontSize: "var(--text-md)",
      lineHeight: 1.4,
      color: "var(--text-body)",
      userSelect: "none",
      ...style
    }
  }, /*#__PURE__*/React.createElement("button", {
    type: "button",
    role: "checkbox",
    id: id,
    "aria-checked": checked,
    onClick: handle,
    disabled: disabled,
    className: "txyz-check",
    style: {
      width: 20,
      height: 20,
      flex: "none",
      marginTop: 1,
      borderRadius: "var(--radius-xs)",
      border: `1.5px solid ${checked ? "var(--color-primary)" : "var(--border-strong)"}`,
      background: checked ? "var(--color-primary)" : "var(--surface-card)",
      cursor: "inherit",
      display: "inline-flex",
      alignItems: "center",
      justifyContent: "center",
      transition: "background var(--duration-fast) var(--ease-standard), border-color var(--duration-fast) var(--ease-standard)"
    }
  }, checked && /*#__PURE__*/React.createElement("svg", {
    width: "12",
    height: "12",
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "#fff",
    strokeWidth: "3.5",
    strokeLinecap: "round",
    strokeLinejoin: "round"
  }, /*#__PURE__*/React.createElement("polyline", {
    points: "20 6 9 17 4 12"
  })), /*#__PURE__*/React.createElement("style", null, `.txyz-check:focus-visible{outline:none;box-shadow:var(--ring-brand)}`)), label);
}
Object.assign(__ds_scope, { Checkbox });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/Checkbox.jsx", error: String((e && e.message) || e) }); }

// components/forms/Input.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const {
  useId
} = React;
/**
 * TechXYZ Input — labeled text field with optional icon, hint and error.
 */
function Input({
  label,
  value,
  onChange,
  placeholder,
  type = "text",
  hint,
  error,
  iconLeft = null,
  disabled = false,
  required = false,
  id,
  style,
  ...rest
}) {
  const autoId = useId();
  const fieldId = id || autoId;
  const invalid = Boolean(error);
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: "6px",
      ...style
    }
  }, label && /*#__PURE__*/React.createElement("label", {
    htmlFor: fieldId,
    style: {
      fontFamily: "var(--font-sans)",
      fontSize: "var(--text-sm)",
      fontWeight: "var(--weight-semibold)",
      color: "var(--text-strong)"
    }
  }, label, required && /*#__PURE__*/React.createElement("span", {
    style: {
      color: "var(--color-danger)",
      marginLeft: 3
    }
  }, "*")), /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative",
      display: "flex",
      alignItems: "center"
    }
  }, iconLeft && /*#__PURE__*/React.createElement("span", {
    "aria-hidden": "true",
    style: {
      position: "absolute",
      left: 12,
      display: "inline-flex",
      color: "var(--text-subtle)",
      pointerEvents: "none"
    }
  }, iconLeft), /*#__PURE__*/React.createElement("input", _extends({
    id: fieldId,
    type: type,
    value: value,
    onChange: onChange,
    placeholder: placeholder,
    disabled: disabled,
    required: required,
    "aria-invalid": invalid,
    className: "txyz-input",
    style: {
      width: "100%",
      height: "var(--control-md)",
      padding: iconLeft ? "0 14px 0 38px" : "0 14px",
      fontFamily: "var(--font-sans)",
      fontSize: "var(--text-md)",
      color: "var(--text-strong)",
      background: disabled ? "var(--surface-sunken)" : "var(--surface-card)",
      border: `1.5px solid ${invalid ? "var(--color-danger)" : "var(--border-default)"}`,
      borderRadius: "var(--radius-md)",
      outline: "none",
      transition: "border-color var(--duration-fast) var(--ease-standard), box-shadow var(--duration-fast) var(--ease-standard)"
    }
  }, rest)), /*#__PURE__*/React.createElement("style", null, `
          .txyz-input::placeholder{color:var(--text-subtle)}
          .txyz-input:hover:not(:disabled):not([aria-invalid="true"]){border-color:var(--border-strong)}
          .txyz-input:focus{border-color:var(--border-focus);box-shadow:var(--ring-brand)}
          .txyz-input[aria-invalid="true"]:focus{box-shadow:0 0 0 3px var(--danger-100)}
        `)), (hint || error) && /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-sans)",
      fontSize: "var(--text-xs)",
      color: invalid ? "var(--color-danger)" : "var(--text-muted)"
    }
  }, error || hint));
}
Object.assign(__ds_scope, { Input });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/Input.jsx", error: String((e && e.message) || e) }); }

// components/forms/Select.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const {
  useId
} = React;
/**
 * TechXYZ Select — labeled native dropdown styled to match Input.
 * `options` is an array of { value, label } or plain strings.
 */
function Select({
  label,
  value,
  onChange,
  options = [],
  placeholder,
  hint,
  error,
  disabled = false,
  required = false,
  id,
  style,
  ...rest
}) {
  const autoId = useId();
  const fieldId = id || autoId;
  const invalid = Boolean(error);
  const opts = options.map(o => typeof o === "string" ? {
    value: o,
    label: o
  } : o);
  return /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: "6px",
      ...style
    }
  }, label && /*#__PURE__*/React.createElement("label", {
    htmlFor: fieldId,
    style: {
      fontFamily: "var(--font-sans)",
      fontSize: "var(--text-sm)",
      fontWeight: "var(--weight-semibold)",
      color: "var(--text-strong)"
    }
  }, label, required && /*#__PURE__*/React.createElement("span", {
    style: {
      color: "var(--color-danger)",
      marginLeft: 3
    }
  }, "*")), /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative",
      display: "flex",
      alignItems: "center"
    }
  }, /*#__PURE__*/React.createElement("select", _extends({
    id: fieldId,
    value: value,
    onChange: onChange,
    disabled: disabled,
    required: required,
    "aria-invalid": invalid,
    className: "txyz-select",
    style: {
      width: "100%",
      height: "var(--control-md)",
      padding: "0 38px 0 14px",
      fontFamily: "var(--font-sans)",
      fontSize: "var(--text-md)",
      color: value ? "var(--text-strong)" : "var(--text-subtle)",
      background: disabled ? "var(--surface-sunken)" : "var(--surface-card)",
      border: `1.5px solid ${invalid ? "var(--color-danger)" : "var(--border-default)"}`,
      borderRadius: "var(--radius-md)",
      appearance: "none",
      WebkitAppearance: "none",
      outline: "none",
      cursor: disabled ? "not-allowed" : "pointer",
      transition: "border-color var(--duration-fast) var(--ease-standard), box-shadow var(--duration-fast) var(--ease-standard)"
    }
  }, rest), placeholder && /*#__PURE__*/React.createElement("option", {
    value: "",
    disabled: true
  }, placeholder), opts.map(o => /*#__PURE__*/React.createElement("option", {
    key: o.value,
    value: o.value
  }, o.label))), /*#__PURE__*/React.createElement("span", {
    "aria-hidden": "true",
    style: {
      position: "absolute",
      right: 13,
      pointerEvents: "none",
      color: "var(--text-muted)",
      display: "inline-flex"
    }
  }, /*#__PURE__*/React.createElement("svg", {
    width: "18",
    height: "18",
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: "2",
    strokeLinecap: "round",
    strokeLinejoin: "round"
  }, /*#__PURE__*/React.createElement("polyline", {
    points: "6 9 12 15 18 9"
  }))), /*#__PURE__*/React.createElement("style", null, `
          .txyz-select:hover:not(:disabled):not([aria-invalid="true"]){border-color:var(--border-strong)}
          .txyz-select:focus{border-color:var(--border-focus);box-shadow:var(--ring-brand)}
        `)), (hint || error) && /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-sans)",
      fontSize: "var(--text-xs)",
      color: invalid ? "var(--color-danger)" : "var(--text-muted)"
    }
  }, error || hint));
}
Object.assign(__ds_scope, { Select });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/Select.jsx", error: String((e && e.message) || e) }); }

// components/forms/Switch.jsx
try { (() => {
/**
 * TechXYZ Switch — an on/off toggle. Controlled via `checked` + `onChange`.
 */
function Switch({
  checked = false,
  onChange,
  label,
  disabled = false,
  id,
  style
}) {
  const handle = () => {
    if (!disabled && onChange) onChange(!checked);
  };
  return /*#__PURE__*/React.createElement("label", {
    style: {
      display: "inline-flex",
      alignItems: "center",
      gap: "10px",
      cursor: disabled ? "not-allowed" : "pointer",
      opacity: disabled ? 0.55 : 1,
      fontFamily: "var(--font-sans)",
      fontSize: "var(--text-md)",
      color: "var(--text-strong)",
      userSelect: "none",
      ...style
    }
  }, /*#__PURE__*/React.createElement("button", {
    type: "button",
    role: "switch",
    id: id,
    "aria-checked": checked,
    onClick: handle,
    disabled: disabled,
    className: "txyz-switch",
    style: {
      width: 44,
      height: 26,
      flex: "none",
      borderRadius: "var(--radius-pill)",
      border: "none",
      padding: 3,
      cursor: "inherit",
      background: checked ? "var(--color-primary)" : "var(--neutral-300)",
      boxShadow: checked ? "var(--shadow-brand)" : "var(--shadow-inset)",
      transition: "background var(--duration-base) var(--ease-standard)",
      display: "inline-flex",
      alignItems: "center"
    }
  }, /*#__PURE__*/React.createElement("span", {
    "aria-hidden": "true",
    style: {
      width: 20,
      height: 20,
      borderRadius: "50%",
      background: "#fff",
      boxShadow: "var(--shadow-sm)",
      transform: checked ? "translateX(18px)" : "translateX(0)",
      transition: "transform var(--duration-base) var(--ease-emphasized)"
    }
  }), /*#__PURE__*/React.createElement("style", null, `.txyz-switch:focus-visible{outline:none;box-shadow:var(--ring-brand)}`)), label);
}
Object.assign(__ds_scope, { Switch });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/forms/Switch.jsx", error: String((e && e.message) || e) }); }

// ui_kits/app/Dashboard.jsx
try { (() => {
/* TechXYZ app — dashboard */
function Dashboard({
  onOpenRequests
}) {
  const {
    Card,
    Badge,
    Button
  } = window.TechXYZDesignSystem_ff9a8f;
  const stats = [["inbox", "Demandes ouvertes", "37", "+5 cette semaine", "brand"], ["clock", "En attente", "12", "délai moyen 2,3 j", "warning"], ["check-circle-2", "Traitées (mois)", "248", "+18 %", "success"], ["users", "Membres actifs", "1 204", "+32", "neutral"]];
  const activity = [["file-text", "Nouvelle demande — État civil", "Mairie · il y a 12 min", "brand", "Reçu"], ["check", "Demande #3041 traitée", "Service voirie · il y a 1 h", "success", "Traité"], ["user-plus", "Nouveau membre inscrit", "Association · il y a 3 h", "neutral", "Inscription"], ["alert-triangle", "Demande en retard #2987", "Urbanisme · il y a 5 h", "warning", "En retard"]];
  const bars = [42, 58, 47, 71, 64, 83, 76];
  const days = ["L", "M", "M", "J", "V", "S", "D"];
  return /*#__PURE__*/React.createElement("div", {
    style: {
      padding: "var(--space-6)",
      display: "flex",
      flexDirection: "column",
      gap: "var(--space-5)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "grid",
      gridTemplateColumns: "repeat(auto-fit, minmax(210px, 1fr))",
      gap: "var(--space-4)"
    }
  }, stats.map(([ic, label, val, delta, tone]) => /*#__PURE__*/React.createElement(Card, {
    key: label,
    padding: "var(--space-5)"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      marginBottom: 14
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 40,
      height: 40,
      borderRadius: "var(--radius-md)",
      background: "var(--azure-50)",
      color: "var(--azure-600)",
      display: "inline-flex",
      alignItems: "center",
      justifyContent: "center"
    }
  }, /*#__PURE__*/React.createElement("i", {
    "data-lucide": ic,
    style: {
      width: 20,
      height: 20
    }
  })), /*#__PURE__*/React.createElement(Badge, {
    tone: tone
  }, delta)), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-display)",
      fontWeight: 800,
      fontSize: 30,
      color: "var(--ink-900)",
      lineHeight: 1
    }
  }, val), /*#__PURE__*/React.createElement("div", {
    style: {
      color: "var(--text-muted)",
      fontSize: "var(--text-sm)",
      marginTop: 6
    }
  }, label)))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "grid",
      gridTemplateColumns: "1.4fr 1fr",
      gap: "var(--space-4)"
    },
    className: "txyz-dash-grid"
  }, /*#__PURE__*/React.createElement(Card, {
    padding: "var(--space-6)"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      marginBottom: 20
    }
  }, /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("h3", {
    style: {
      fontSize: "var(--text-lg)",
      margin: 0,
      color: "var(--ink-900)"
    }
  }, "Demandes re\xE7ues"), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: "var(--text-xs)",
      color: "var(--text-muted)"
    }
  }, "7 derniers jours")), /*#__PURE__*/React.createElement(Badge, {
    tone: "success",
    dot: true
  }, "En hausse")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "flex-end",
      gap: 14,
      height: 160
    }
  }, bars.map((h, i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      flex: 1,
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      gap: 8
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      width: "100%",
      maxWidth: 34,
      height: h * 1.6,
      borderRadius: "6px 6px 0 0",
      background: i === bars.length - 1 ? "var(--gradient-spark)" : "var(--azure-200)"
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: 11,
      color: "var(--text-subtle)",
      fontFamily: "var(--font-mono)"
    }
  }, days[i]))))), /*#__PURE__*/React.createElement(Card, {
    padding: "var(--space-5)"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      marginBottom: 12
    }
  }, /*#__PURE__*/React.createElement("h3", {
    style: {
      fontSize: "var(--text-lg)",
      margin: 0,
      color: "var(--ink-900)"
    }
  }, "Activit\xE9 r\xE9cente"), /*#__PURE__*/React.createElement(Button, {
    variant: "ghost",
    size: "sm",
    onClick: onOpenRequests
  }, "Tout voir")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column"
    }
  }, activity.map(([ic, title, meta, tone, st], i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      display: "flex",
      alignItems: "center",
      gap: 12,
      padding: "11px 0",
      borderTop: i ? "1px solid var(--border-subtle)" : "none"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 34,
      height: 34,
      flex: "none",
      borderRadius: 9,
      background: "var(--surface-sunken)",
      color: "var(--text-muted)",
      display: "inline-flex",
      alignItems: "center",
      justifyContent: "center"
    }
  }, /*#__PURE__*/React.createElement("i", {
    "data-lucide": ic,
    style: {
      width: 17,
      height: 17
    }
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      minWidth: 0,
      flex: 1
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 13.5,
      fontWeight: 600,
      color: "var(--ink-900)",
      whiteSpace: "nowrap",
      overflow: "hidden",
      textOverflow: "ellipsis"
    }
  }, title), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 12,
      color: "var(--text-subtle)"
    }
  }, meta)), /*#__PURE__*/React.createElement(Badge, {
    tone: tone
  }, st)))))), /*#__PURE__*/React.createElement("style", null, `@media (max-width: 900px){ .txyz-dash-grid{ grid-template-columns: 1fr !important; } }`));
}
Object.assign(window, {
  Dashboard
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/app/Dashboard.jsx", error: String((e && e.message) || e) }); }

// ui_kits/app/LoginScreen.jsx
try { (() => {
/* TechXYZ app — login screen */
function LoginScreen({
  onLogin
}) {
  const {
    Input,
    Button,
    Checkbox
  } = window.TechXYZDesignSystem_ff9a8f;
  const [email, setEmail] = React.useState("camille@ville-exemple.fr");
  const [pw, setPw] = React.useState("");
  const [remember, setRemember] = React.useState(true);
  const submit = e => {
    e.preventDefault();
    onLogin();
  };
  return /*#__PURE__*/React.createElement("div", {
    style: {
      minHeight: "100%",
      display: "grid",
      gridTemplateColumns: "1fr 1fr"
    },
    className: "txyz-login"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative",
      overflow: "hidden",
      background: "var(--gradient-mesh)",
      color: "#fff",
      padding: "var(--space-9)",
      display: "flex",
      flexDirection: "column",
      justifyContent: "space-between"
    },
    className: "txyz-login-brand"
  }, /*#__PURE__*/React.createElement("div", {
    "aria-hidden": "true",
    style: {
      position: "absolute",
      inset: 0,
      backgroundImage: "radial-gradient(circle at 1px 1px, rgba(255,255,255,0.05) 1px, transparent 0)",
      backgroundSize: "26px 26px"
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative",
      display: "flex",
      alignItems: "center",
      gap: 12
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: "../../assets/logo/techxyz-mark-dark.png",
    alt: "",
    style: {
      width: 40,
      height: 40
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-display)",
      fontWeight: 700,
      fontSize: 22
    }
  }, "TECH", /*#__PURE__*/React.createElement("span", {
    style: {
      color: "var(--azure-400)",
      fontWeight: 500
    }
  }, "XYZ"))), /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative"
    }
  }, /*#__PURE__*/React.createElement("h2", {
    style: {
      fontSize: "var(--text-3xl)",
      color: "#fff",
      lineHeight: 1.2,
      margin: "0 0 14px"
    }
  }, "Votre espace d'administration"), /*#__PURE__*/React.createElement("p", {
    style: {
      color: "rgba(255,255,255,0.72)",
      fontSize: "var(--text-lg)",
      maxWidth: "38ch",
      margin: 0
    }
  }, "G\xE9rez les demandes, votre r\xE9pertoire et vos documents depuis un seul endroit, en toute s\xE9curit\xE9.")), /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative",
      fontSize: "var(--text-xs)",
      color: "rgba(255,255,255,0.5)"
    }
  }, "H\xE9berg\xE9 en France \xB7 Conforme RGPD")), /*#__PURE__*/React.createElement("div", {
    style: {
      background: "var(--surface-page)",
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      padding: "var(--space-7)"
    }
  }, /*#__PURE__*/React.createElement("form", {
    onSubmit: submit,
    style: {
      width: "100%",
      maxWidth: 360,
      display: "flex",
      flexDirection: "column",
      gap: 18
    }
  }, /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("h1", {
    style: {
      fontSize: "var(--text-2xl)",
      color: "var(--ink-900)",
      margin: "0 0 6px"
    }
  }, "Connexion"), /*#__PURE__*/React.createElement("p", {
    style: {
      color: "var(--text-muted)",
      margin: 0,
      fontSize: "var(--text-sm)"
    }
  }, "Acc\xE9dez \xE0 votre tableau de bord.")), /*#__PURE__*/React.createElement(Input, {
    label: "E-mail",
    type: "email",
    value: email,
    onChange: e => setEmail(e.target.value),
    iconLeft: /*#__PURE__*/React.createElement("i", {
      "data-lucide": "mail"
    }),
    required: true
  }), /*#__PURE__*/React.createElement(Input, {
    label: "Mot de passe",
    type: "password",
    value: pw,
    onChange: e => setPw(e.target.value),
    placeholder: "\u2022\u2022\u2022\u2022\u2022\u2022\u2022\u2022",
    iconLeft: /*#__PURE__*/React.createElement("i", {
      "data-lucide": "lock"
    }),
    required: true
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between"
    }
  }, /*#__PURE__*/React.createElement(Checkbox, {
    checked: remember,
    onChange: setRemember,
    label: "Se souvenir de moi"
  }), /*#__PURE__*/React.createElement("a", {
    href: "#",
    style: {
      fontSize: "var(--text-sm)",
      fontWeight: 600
    }
  }, "Mot de passe oubli\xE9 ?")), /*#__PURE__*/React.createElement(Button, {
    type: "submit",
    variant: "primary",
    size: "lg",
    iconRight: /*#__PURE__*/React.createElement("i", {
      "data-lucide": "arrow-right"
    })
  }, "Se connecter"))), /*#__PURE__*/React.createElement("style", null, `@media (max-width: 820px){ .txyz-login{ grid-template-columns: 1fr !important; } .txyz-login-brand{ display:none !important; } }`));
}
Object.assign(window, {
  LoginScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/app/LoginScreen.jsx", error: String((e && e.message) || e) }); }

// ui_kits/app/RequestDetail.jsx
try { (() => {
/* TechXYZ app — request detail */
function RequestDetail({
  request,
  onBack
}) {
  const {
    Card,
    Badge,
    Button,
    Select,
    Avatar
  } = window.TechXYZDesignSystem_ff9a8f;
  const r = request || ["#3052", "Acte de naissance", "M. Bernard Lefèvre", "État civil", "Reçu", "brand", "Aujourd'hui"];
  const timeline = [["Demande reçue", "Aujourd'hui · 09:14", "via le portail citoyen", true], ["Affectée au service", r[3], "par Camille Durand", true], ["En cours de traitement", "—", "en attente de pièce justificative", false], ["Demande clôturée", "—", "", false]];
  return /*#__PURE__*/React.createElement("div", {
    style: {
      padding: "var(--space-6)",
      maxWidth: 980,
      margin: "0 auto",
      display: "flex",
      flexDirection: "column",
      gap: "var(--space-4)"
    }
  }, /*#__PURE__*/React.createElement("button", {
    onClick: onBack,
    style: {
      display: "inline-flex",
      alignItems: "center",
      gap: 6,
      background: "none",
      border: "none",
      cursor: "pointer",
      color: "var(--text-muted)",
      fontFamily: "var(--font-sans)",
      fontWeight: 600,
      fontSize: "var(--text-sm)",
      padding: 0,
      alignSelf: "flex-start"
    }
  }, /*#__PURE__*/React.createElement("i", {
    "data-lucide": "arrow-left",
    style: {
      width: 16,
      height: 16
    }
  }), " Retour aux demandes"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "flex-start",
      justifyContent: "space-between",
      gap: 16,
      flexWrap: "wrap"
    }
  }, /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 10,
      marginBottom: 6
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-mono)",
      fontSize: 13,
      color: "var(--text-muted)"
    }
  }, r[0]), /*#__PURE__*/React.createElement(Badge, {
    tone: r[5],
    dot: true
  }, r[4])), /*#__PURE__*/React.createElement("h1", {
    style: {
      fontSize: "var(--text-2xl)",
      margin: 0,
      color: "var(--ink-900)"
    }
  }, r[1])), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 10
    }
  }, /*#__PURE__*/React.createElement(Button, {
    variant: "outline",
    iconLeft: /*#__PURE__*/React.createElement("i", {
      "data-lucide": "message-circle"
    })
  }, "Contacter"), /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    iconLeft: /*#__PURE__*/React.createElement("i", {
      "data-lucide": "check"
    })
  }, "Marquer trait\xE9e"))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "grid",
      gridTemplateColumns: "1.5fr 1fr",
      gap: "var(--space-4)"
    },
    className: "txyz-detail-grid"
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: "var(--space-4)"
    }
  }, /*#__PURE__*/React.createElement(Card, {
    padding: "var(--space-6)"
  }, /*#__PURE__*/React.createElement("h3", {
    style: {
      fontSize: "var(--text-md)",
      margin: "0 0 16px",
      color: "var(--text-muted)",
      textTransform: "uppercase",
      letterSpacing: "0.04em",
      fontSize: 12,
      fontWeight: 700
    }
  }, "D\xE9tails de la demande"), /*#__PURE__*/React.createElement("dl", {
    style: {
      margin: 0,
      display: "grid",
      gridTemplateColumns: "auto 1fr",
      gap: "12px 24px"
    }
  }, [["Demandeur", r[2]], ["Service concerné", r[3]], ["Canal", "Portail citoyen"], ["Reçue le", r[6] + " · 09:14"], ["Pièces jointes", "2 documents"]].map(([k, v]) => /*#__PURE__*/React.createElement(React.Fragment, {
    key: k
  }, /*#__PURE__*/React.createElement("dt", {
    style: {
      color: "var(--text-muted)",
      fontSize: "var(--text-sm)"
    }
  }, k), /*#__PURE__*/React.createElement("dd", {
    style: {
      margin: 0,
      color: "var(--ink-900)",
      fontSize: "var(--text-sm)",
      fontWeight: 600
    }
  }, v)))), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 18,
      paddingTop: 18,
      borderTop: "1px solid var(--border-subtle)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      color: "var(--text-muted)",
      fontSize: "var(--text-sm)",
      marginBottom: 8
    }
  }, "Message du demandeur"), /*#__PURE__*/React.createElement("p", {
    style: {
      margin: 0,
      color: "var(--text-body)",
      fontSize: "var(--text-md)",
      lineHeight: 1.6
    }
  }, "\xAB Bonjour, je souhaiterais obtenir un acte de naissance pour une d\xE9marche administrative. Merci d'avance pour votre aide. \xBB"))), /*#__PURE__*/React.createElement(Card, {
    padding: "var(--space-6)"
  }, /*#__PURE__*/React.createElement("h3", {
    style: {
      fontSize: 12,
      fontWeight: 700,
      margin: "0 0 18px",
      color: "var(--text-muted)",
      textTransform: "uppercase",
      letterSpacing: "0.04em"
    }
  }, "Suivi"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column"
    }
  }, timeline.map(([title, time, note, done], i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      display: "flex",
      gap: 14,
      paddingBottom: i < timeline.length - 1 ? 18 : 0
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      alignItems: "center"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 14,
      height: 14,
      borderRadius: "50%",
      flex: "none",
      background: done ? "var(--azure-500)" : "#fff",
      border: `2px solid ${done ? "var(--azure-500)" : "var(--border-default)"}`
    }
  }), i < timeline.length - 1 && /*#__PURE__*/React.createElement("span", {
    style: {
      width: 2,
      flex: 1,
      background: done ? "var(--azure-200)" : "var(--border-subtle)",
      marginTop: 2
    }
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: -3
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontWeight: 600,
      color: done ? "var(--ink-900)" : "var(--text-muted)",
      fontSize: "var(--text-sm)"
    }
  }, title), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 12,
      color: "var(--text-subtle)"
    }
  }, time, note ? " · " + note : ""))))))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: "var(--space-4)"
    }
  }, /*#__PURE__*/React.createElement(Card, {
    padding: "var(--space-5)"
  }, /*#__PURE__*/React.createElement("h3", {
    style: {
      fontSize: 12,
      fontWeight: 700,
      margin: "0 0 14px",
      color: "var(--text-muted)",
      textTransform: "uppercase",
      letterSpacing: "0.04em"
    }
  }, "Affectation"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 10,
      marginBottom: 16
    }
  }, /*#__PURE__*/React.createElement(Avatar, {
    name: "Camille Durand",
    size: "sm"
  }), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--text-sm)",
      fontWeight: 600,
      color: "var(--ink-900)"
    }
  }, "Camille Durand"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 12,
      color: "var(--text-subtle)"
    }
  }, "Responsable ", r[3]))), /*#__PURE__*/React.createElement(Select, {
    label: "R\xE9affecter \xE0",
    placeholder: "Choisir un agent\u2026",
    options: ["Camille Durand", "Yann Morel", "Sofia Nguyen"]
  })), /*#__PURE__*/React.createElement(Card, {
    padding: "var(--space-5)"
  }, /*#__PURE__*/React.createElement("h3", {
    style: {
      fontSize: 12,
      fontWeight: 700,
      margin: "0 0 12px",
      color: "var(--text-muted)",
      textTransform: "uppercase",
      letterSpacing: "0.04em"
    }
  }, "Note interne"), /*#__PURE__*/React.createElement("textarea", {
    placeholder: "Ajouter une note visible par l'\xE9quipe\u2026",
    rows: 3,
    style: {
      width: "100%",
      resize: "vertical",
      padding: "10px 12px",
      fontFamily: "var(--font-sans)",
      fontSize: "var(--text-sm)",
      color: "var(--text-strong)",
      border: "1.5px solid var(--border-default)",
      borderRadius: "var(--radius-md)",
      outline: "none"
    }
  }), /*#__PURE__*/React.createElement(Button, {
    variant: "secondary",
    size: "sm",
    style: {
      marginTop: 10,
      width: "100%"
    }
  }, "Enregistrer la note")))), /*#__PURE__*/React.createElement("style", null, `@media (max-width: 880px){ .txyz-detail-grid{ grid-template-columns: 1fr !important; } }`));
}
Object.assign(window, {
  RequestDetail
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/app/RequestDetail.jsx", error: String((e && e.message) || e) }); }

// ui_kits/app/RequestsList.jsx
try { (() => {
/* TechXYZ app — requests list (table view) */
function RequestsList({
  onOpen
}) {
  const {
    Card,
    Badge,
    Button,
    Input,
    Select,
    IconButton
  } = window.TechXYZDesignSystem_ff9a8f;
  const [filter, setFilter] = React.useState("");
  const rows = [["#3052", "Acte de naissance", "M. Bernard Lefèvre", "État civil", "Reçu", "brand", "Aujourd'hui"], ["#3051", "Permis de construire", "SCI Horizon", "Urbanisme", "En cours", "warning", "Aujourd'hui"], ["#3049", "Réservation salle des fêtes", "Asso. Les Coquelicots", "Réservations", "Traité", "success", "Hier"], ["#3047", "Signalement voirie", "Mme Claire Dubois", "Voirie", "En cours", "warning", "Hier"], ["#3044", "Inscription cantine", "M. Karim Benali", "Scolaire", "Traité", "success", "12 mars"], ["#3041", "Subvention association", "Asso. Sport Pour Tous", "Finances", "En retard", "danger", "11 mars"]];
  const visible = rows.filter(r => !filter || r[1].toLowerCase().includes(filter.toLowerCase()) || r[2].toLowerCase().includes(filter.toLowerCase()));
  return /*#__PURE__*/React.createElement("div", {
    style: {
      padding: "var(--space-6)",
      display: "flex",
      flexDirection: "column",
      gap: "var(--space-4)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 12,
      flexWrap: "wrap"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      width: 260
    }
  }, /*#__PURE__*/React.createElement(Input, {
    placeholder: "Filtrer par objet ou demandeur\u2026",
    iconLeft: /*#__PURE__*/React.createElement("i", {
      "data-lucide": "search"
    }),
    value: filter,
    onChange: e => setFilter(e.target.value)
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      width: 180
    }
  }, /*#__PURE__*/React.createElement(Select, {
    placeholder: "Tous les statuts",
    options: ["Reçu", "En cours", "Traité", "En retard"]
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1
    }
  }), /*#__PURE__*/React.createElement(Button, {
    variant: "outline",
    iconLeft: /*#__PURE__*/React.createElement("i", {
      "data-lucide": "download"
    })
  }, "Exporter"), /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    iconLeft: /*#__PURE__*/React.createElement("i", {
      "data-lucide": "plus"
    })
  }, "Nouvelle demande")), /*#__PURE__*/React.createElement(Card, {
    padding: "0",
    style: {
      overflow: "hidden"
    }
  }, /*#__PURE__*/React.createElement("table", {
    style: {
      width: "100%",
      borderCollapse: "collapse",
      fontFamily: "var(--font-sans)"
    }
  }, /*#__PURE__*/React.createElement("thead", null, /*#__PURE__*/React.createElement("tr", {
    style: {
      background: "var(--surface-sunken)"
    }
  }, ["Réf.", "Objet", "Demandeur", "Service", "Statut", "Date", ""].map((h, i) => /*#__PURE__*/React.createElement("th", {
    key: i,
    style: {
      textAlign: "left",
      padding: "12px 16px",
      fontSize: 12,
      fontWeight: 700,
      color: "var(--text-muted)",
      letterSpacing: "0.02em",
      textTransform: "uppercase",
      borderBottom: "1px solid var(--border-subtle)",
      whiteSpace: "nowrap"
    }
  }, h)))), /*#__PURE__*/React.createElement("tbody", null, visible.map((r, i) => /*#__PURE__*/React.createElement("tr", {
    key: r[0],
    className: "txyz-row",
    onClick: () => onOpen(r),
    style: {
      cursor: "pointer",
      borderBottom: i < visible.length - 1 ? "1px solid var(--border-subtle)" : "none",
      transition: "background var(--duration-fast) var(--ease-standard)"
    }
  }, /*#__PURE__*/React.createElement("td", {
    style: {
      padding: "13px 16px",
      fontFamily: "var(--font-mono)",
      fontSize: 13,
      color: "var(--text-muted)",
      whiteSpace: "nowrap"
    }
  }, r[0]), /*#__PURE__*/React.createElement("td", {
    style: {
      padding: "13px 16px",
      fontWeight: 600,
      color: "var(--ink-900)",
      fontSize: 14
    }
  }, r[1]), /*#__PURE__*/React.createElement("td", {
    style: {
      padding: "13px 16px",
      color: "var(--text-body)",
      fontSize: 14,
      whiteSpace: "nowrap"
    }
  }, r[2]), /*#__PURE__*/React.createElement("td", {
    style: {
      padding: "13px 16px",
      color: "var(--text-muted)",
      fontSize: 14,
      whiteSpace: "nowrap"
    }
  }, r[3]), /*#__PURE__*/React.createElement("td", {
    style: {
      padding: "13px 16px"
    }
  }, /*#__PURE__*/React.createElement(Badge, {
    tone: r[5],
    dot: true
  }, r[4])), /*#__PURE__*/React.createElement("td", {
    style: {
      padding: "13px 16px",
      color: "var(--text-subtle)",
      fontSize: 13,
      whiteSpace: "nowrap"
    }
  }, r[6]), /*#__PURE__*/React.createElement("td", {
    style: {
      padding: "13px 8px",
      textAlign: "right"
    }
  }, /*#__PURE__*/React.createElement("i", {
    "data-lucide": "chevron-right",
    style: {
      width: 18,
      height: 18,
      color: "var(--text-subtle)"
    }
  }))))))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      color: "var(--text-muted)",
      fontSize: "var(--text-sm)"
    }
  }, /*#__PURE__*/React.createElement("span", null, visible.length, " demande", visible.length > 1 ? "s" : "", " affich\xE9e", visible.length > 1 ? "s" : ""), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 6
    }
  }, /*#__PURE__*/React.createElement(IconButton, {
    label: "Pr\xE9c\xE9dent",
    variant: "outline"
  }, /*#__PURE__*/React.createElement("i", {
    "data-lucide": "chevron-left"
  })), /*#__PURE__*/React.createElement(IconButton, {
    label: "Suivant",
    variant: "outline"
  }, /*#__PURE__*/React.createElement("i", {
    "data-lucide": "chevron-right"
  })))), /*#__PURE__*/React.createElement("style", null, `.txyz-row:hover{ background: var(--surface-page); }`));
}
Object.assign(window, {
  RequestsList
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/app/RequestsList.jsx", error: String((e && e.message) || e) }); }

// ui_kits/app/Sidebar.jsx
try { (() => {
/* TechXYZ app — left navigation rail (ink ground) */
function Sidebar({
  active,
  onNavigate,
  org
}) {
  const nav = [["dashboard", "layout-dashboard", "Tableau de bord"], ["requests", "inbox", "Demandes"], ["directory", "users", "Répertoire"], ["calendar", "calendar-days", "Agenda"], ["documents", "folder", "Documents"]];
  const bottom = [["settings", "settings", "Paramètres"], ["help", "life-buoy", "Aide"]];
  const Item = ([key, icon, label]) => /*#__PURE__*/React.createElement("button", {
    key: key,
    onClick: () => onNavigate(key),
    className: "txyz-navitem",
    "data-active": active === key,
    style: {
      display: "flex",
      alignItems: "center",
      gap: 12,
      width: "100%",
      padding: "10px 12px",
      borderRadius: "var(--radius-md)",
      border: "none",
      background: active === key ? "var(--azure-500)" : "transparent",
      color: active === key ? "#fff" : "rgba(255,255,255,0.66)",
      boxShadow: active === key ? "var(--shadow-brand)" : "none",
      fontFamily: "var(--font-sans)",
      fontWeight: 600,
      fontSize: "var(--text-sm)",
      cursor: "pointer",
      textAlign: "left",
      transition: "background var(--duration-fast) var(--ease-standard), color var(--duration-fast) var(--ease-standard)"
    }
  }, /*#__PURE__*/React.createElement("i", {
    "data-lucide": icon,
    style: {
      width: 19,
      height: 19,
      strokeWidth: 1.9
    }
  }), label);
  return /*#__PURE__*/React.createElement("aside", {
    style: {
      width: 252,
      flex: "none",
      background: "var(--ink-900)",
      color: "#fff",
      display: "flex",
      flexDirection: "column",
      padding: "20px 14px",
      borderRight: "1px solid rgba(255,255,255,0.06)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 10,
      padding: "4px 8px 22px"
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: "../../assets/logo/techxyz-mark-dark.png",
    alt: "",
    style: {
      width: 34,
      height: 34
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-display)",
      fontWeight: 700,
      fontSize: 18,
      color: "#fff"
    }
  }, "TECH", /*#__PURE__*/React.createElement("span", {
    style: {
      color: "var(--azure-500)",
      fontWeight: 500
    }
  }, "XYZ"))), /*#__PURE__*/React.createElement("nav", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 4
    }
  }, nav.map(Item)), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1
    }
  }), /*#__PURE__*/React.createElement("nav", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 4,
      marginBottom: 12
    }
  }, bottom.map(Item)), /*#__PURE__*/React.createElement("div", {
    style: {
      borderTop: "1px solid rgba(255,255,255,0.08)",
      paddingTop: 12,
      display: "flex",
      alignItems: "center",
      gap: 10
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 36,
      height: 36,
      borderRadius: 9,
      background: "rgba(255,255,255,0.08)",
      display: "inline-flex",
      alignItems: "center",
      justifyContent: "center",
      color: "var(--azure-400)",
      flex: "none"
    }
  }, /*#__PURE__*/React.createElement("i", {
    "data-lucide": "building-2",
    style: {
      width: 18,
      height: 18
    }
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      minWidth: 0
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 13,
      fontWeight: 700,
      color: "#fff",
      whiteSpace: "nowrap",
      overflow: "hidden",
      textOverflow: "ellipsis"
    }
  }, org), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 11,
      color: "rgba(255,255,255,0.5)"
    }
  }, "Espace administrateur"))), /*#__PURE__*/React.createElement("style", null, `.txyz-navitem:hover[data-active="false"]{ background: rgba(255,255,255,0.06) !important; color:#fff !important; }`));
}
Object.assign(window, {
  Sidebar
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/app/Sidebar.jsx", error: String((e && e.message) || e) }); }

// ui_kits/app/Topbar.jsx
try { (() => {
/* TechXYZ app — top bar */
function Topbar({
  title,
  subtitle,
  onLogout
}) {
  const {
    Input,
    IconButton,
    Avatar
  } = window.TechXYZDesignSystem_ff9a8f;
  return /*#__PURE__*/React.createElement("header", {
    style: {
      height: 72,
      flex: "none",
      background: "#fff",
      borderBottom: "1px solid var(--border-subtle)",
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      padding: "0 var(--space-6)"
    }
  }, /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("h1", {
    style: {
      fontSize: "var(--text-xl)",
      margin: 0,
      color: "var(--ink-900)",
      fontWeight: 700
    }
  }, title), subtitle && /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--text-xs)",
      color: "var(--text-muted)",
      marginTop: 2
    }
  }, subtitle)), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 14
    }
  }, /*#__PURE__*/React.createElement("div", {
    className: "txyz-topsearch",
    style: {
      width: 240
    }
  }, /*#__PURE__*/React.createElement(Input, {
    placeholder: "Rechercher\u2026",
    iconLeft: /*#__PURE__*/React.createElement("i", {
      "data-lucide": "search"
    })
  })), /*#__PURE__*/React.createElement(IconButton, {
    label: "Notifications",
    variant: "ghost"
  }, /*#__PURE__*/React.createElement("i", {
    "data-lucide": "bell"
  })), /*#__PURE__*/React.createElement("button", {
    onClick: onLogout,
    title: "Profil",
    style: {
      border: "none",
      background: "transparent",
      cursor: "pointer",
      padding: 0,
      display: "flex",
      alignItems: "center",
      gap: 8
    }
  }, /*#__PURE__*/React.createElement(Avatar, {
    name: "Camille Durand",
    size: "sm"
  }))), /*#__PURE__*/React.createElement("style", null, `@media (max-width: 720px){ .txyz-topsearch{ display:none; } }`));
}
Object.assign(window, {
  Topbar
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/app/Topbar.jsx", error: String((e && e.message) || e) }); }

// ui_kits/vitrine/Audiences.jsx
try { (() => {
/* TechXYZ vitrine — audiences ("pour qui ?") */
function Audiences() {
  const {
    Card
  } = window.TechXYZDesignSystem_ff9a8f;
  const items = [["building-2", "Municipalités & collectivités", "Portails citoyens, gestion des demandes, réservation de salles, communication. Des outils qui allègent le quotidien de vos agents."], ["users", "Associations", "Adhésions, cotisations, événements et bénévoles — centralisés dans un espace simple, accessible à toute votre équipe."], ["briefcase", "PME & TPE", "Devis, suivi clients, planning, facturation. Le logiciel qui correspond à votre métier, pas l'inverse."]];
  return /*#__PURE__*/React.createElement("section", {
    id: "audiences",
    style: {
      background: "var(--surface-page)",
      padding: "var(--section-y) var(--space-5)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: "var(--container-xl)",
      margin: "0 auto"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      textAlign: "center",
      maxWidth: "62ch",
      margin: "0 auto var(--space-8)"
    }
  }, /*#__PURE__*/React.createElement("span", {
    className: "eyebrow"
  }, "Pour qui ?"), /*#__PURE__*/React.createElement("h2", {
    style: {
      fontSize: "var(--text-3xl)",
      margin: "10px 0 12px",
      color: "var(--ink-900)"
    }
  }, "Con\xE7u pour ceux qui font tourner le territoire"), /*#__PURE__*/React.createElement("p", {
    style: {
      color: "var(--text-muted)",
      fontSize: "var(--text-lg)"
    }
  }, "Trois mondes, une m\xEAme exigence : des outils fiables, sobres et faciles \xE0 adopter.")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "grid",
      gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))",
      gap: "var(--space-5)"
    }
  }, items.map(([ic, title, body], i) => /*#__PURE__*/React.createElement(Card, {
    key: i,
    interactive: true,
    accent: i === 0,
    padding: "var(--space-6)"
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 52,
      height: 52,
      borderRadius: "var(--radius-md)",
      background: "var(--azure-50)",
      display: "inline-flex",
      alignItems: "center",
      justifyContent: "center",
      color: "var(--azure-600)",
      marginBottom: 18
    }
  }, /*#__PURE__*/React.createElement("i", {
    "data-lucide": ic,
    style: {
      width: 26,
      height: 26
    }
  })), /*#__PURE__*/React.createElement("h3", {
    style: {
      fontSize: "var(--text-xl)",
      margin: "0 0 8px",
      color: "var(--ink-900)"
    }
  }, title), /*#__PURE__*/React.createElement("p", {
    style: {
      color: "var(--text-muted)",
      fontSize: "var(--text-md)",
      lineHeight: 1.6,
      margin: 0
    }
  }, body))))));
}
Object.assign(window, {
  Audiences
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/vitrine/Audiences.jsx", error: String((e && e.message) || e) }); }

// ui_kits/vitrine/ContactCTA.jsx
try { (() => {
/* TechXYZ vitrine — contact CTA band with a working (mock) form */
function ContactCTA({
  formRef
}) {
  const {
    Input,
    Select,
    Button,
    Card
  } = window.TechXYZDesignSystem_ff9a8f;
  const [sent, setSent] = React.useState(false);
  const [name, setName] = React.useState("");
  const [email, setEmail] = React.useState("");
  const [org, setOrg] = React.useState("");
  const submit = e => {
    e.preventDefault();
    setSent(true);
  };
  return /*#__PURE__*/React.createElement("section", {
    id: "contact",
    ref: formRef,
    style: {
      background: "var(--gradient-ink)",
      padding: "var(--section-y) var(--space-5)",
      color: "#fff"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: "var(--container-lg)",
      margin: "0 auto",
      display: "grid",
      gridTemplateColumns: "1fr 1fr",
      gap: "var(--space-9)",
      alignItems: "center"
    },
    className: "txyz-cta-grid"
  }, /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-sans)",
      fontWeight: 600,
      fontSize: 12,
      letterSpacing: "0.14em",
      color: "var(--azure-400)"
    }
  }, "PARLONS-EN"), /*#__PURE__*/React.createElement("h2", {
    style: {
      fontSize: "var(--text-3xl)",
      color: "#fff",
      margin: "12px 0 14px",
      lineHeight: 1.15
    }
  }, "Un projet, une question, une id\xE9e ?"), /*#__PURE__*/React.createElement("p", {
    style: {
      color: "rgba(255,255,255,0.75)",
      fontSize: "var(--text-lg)",
      lineHeight: 1.6,
      maxWidth: "42ch"
    }
  }, "D\xE9crivez-nous votre besoin en quelques mots. Nous revenons vers vous sous 48 h, sans engagement."), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 12,
      marginTop: "var(--space-7)"
    }
  }, [["mail", "contact@techxyz.fr"], ["phone", "01 23 45 67 89"], ["map-pin", "France — à distance & sur site"]].map(([ic, t]) => /*#__PURE__*/React.createElement("div", {
    key: t,
    style: {
      display: "flex",
      alignItems: "center",
      gap: 12,
      color: "rgba(255,255,255,0.82)",
      fontSize: "var(--text-md)"
    }
  }, /*#__PURE__*/React.createElement("i", {
    "data-lucide": ic,
    style: {
      width: 18,
      height: 18,
      color: "var(--azure-400)"
    }
  }), t)))), /*#__PURE__*/React.createElement(Card, {
    padding: "var(--space-6)",
    style: {
      borderColor: "transparent"
    }
  }, sent ? /*#__PURE__*/React.createElement("div", {
    style: {
      textAlign: "center",
      padding: "var(--space-6) var(--space-3)"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 56,
      height: 56,
      borderRadius: "50%",
      background: "var(--success-50)",
      color: "var(--success-600)",
      display: "inline-flex",
      alignItems: "center",
      justifyContent: "center",
      marginBottom: 16
    }
  }, /*#__PURE__*/React.createElement("i", {
    "data-lucide": "check",
    style: {
      width: 28,
      height: 28
    }
  })), /*#__PURE__*/React.createElement("h3", {
    style: {
      fontSize: "var(--text-xl)",
      margin: "0 0 8px",
      color: "var(--ink-900)"
    }
  }, "Message bien re\xE7u, merci !"), /*#__PURE__*/React.createElement("p", {
    style: {
      color: "var(--text-muted)",
      margin: 0
    }
  }, "Nous revenons vers vous tr\xE8s vite."), /*#__PURE__*/React.createElement(Button, {
    variant: "ghost",
    onClick: () => setSent(false),
    style: {
      marginTop: 16
    }
  }, "Envoyer un autre message")) : /*#__PURE__*/React.createElement("form", {
    onSubmit: submit,
    style: {
      display: "flex",
      flexDirection: "column",
      gap: 16
    }
  }, /*#__PURE__*/React.createElement(Input, {
    label: "Votre nom",
    placeholder: "Camille Durand",
    value: name,
    onChange: e => setName(e.target.value),
    required: true
  }), /*#__PURE__*/React.createElement(Input, {
    label: "E-mail professionnel",
    type: "email",
    placeholder: "vous@organisation.fr",
    iconLeft: /*#__PURE__*/React.createElement("i", {
      "data-lucide": "mail"
    }),
    value: email,
    onChange: e => setEmail(e.target.value),
    required: true
  }), /*#__PURE__*/React.createElement(Select, {
    label: "Type d'organisation",
    placeholder: "Choisissez\u2026",
    value: org,
    onChange: e => setOrg(e.target.value),
    options: ["Municipalité / collectivité", "Association", "PME / TPE", "Autre"]
  }), /*#__PURE__*/React.createElement(Button, {
    type: "submit",
    variant: "primary",
    size: "lg",
    iconRight: /*#__PURE__*/React.createElement("i", {
      "data-lucide": "send"
    })
  }, "Envoyer ma demande"), /*#__PURE__*/React.createElement("p", {
    style: {
      fontSize: "var(--text-xs)",
      color: "var(--text-subtle)",
      margin: 0,
      textAlign: "center"
    }
  }, "En envoyant ce formulaire, vous acceptez d'\xEAtre recontact\xE9. Aucune donn\xE9e n'est partag\xE9e.")))), /*#__PURE__*/React.createElement("style", null, `@media (max-width: 820px){ .txyz-cta-grid{ grid-template-columns: 1fr !important; } }`));
}
Object.assign(window, {
  ContactCTA
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/vitrine/ContactCTA.jsx", error: String((e && e.message) || e) }); }

// ui_kits/vitrine/Footer.jsx
try { (() => {
/* TechXYZ vitrine — footer */
function Footer() {
  const cols = [["Services", ["Conception sur-mesure", "Développement", "Hébergement RGPD", "Maintenance"]], ["Pour qui ?", ["Municipalités", "Associations", "PME & TPE"]], ["Studio", ["Notre approche", "Réalisations", "Contact"]]];
  return /*#__PURE__*/React.createElement("footer", {
    style: {
      background: "var(--ink-950)",
      color: "rgba(255,255,255,0.7)",
      padding: "var(--space-9) var(--space-5) var(--space-6)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: "var(--container-xl)",
      margin: "0 auto"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "grid",
      gridTemplateColumns: "1.4fr 1fr 1fr 1fr",
      gap: "var(--space-7)",
      paddingBottom: "var(--space-7)"
    },
    className: "txyz-foot-grid"
  }, /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 10,
      marginBottom: 14
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: "../../assets/logo/techxyz-mark-dark.png",
    alt: "",
    style: {
      width: 36,
      height: 36
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-display)",
      fontWeight: 700,
      fontSize: 20,
      color: "#fff"
    }
  }, "TECH", /*#__PURE__*/React.createElement("span", {
    style: {
      color: "var(--azure-500)",
      fontWeight: 500
    }
  }, "XYZ"))), /*#__PURE__*/React.createElement("p", {
    style: {
      fontSize: "var(--text-sm)",
      lineHeight: 1.6,
      maxWidth: "34ch",
      margin: 0
    }
  }, "Studio d'ing\xE9nierie cr\xE9ative. Des logiciels utiles pour les collectivit\xE9s, les associations et les PME.")), cols.map(([title, links]) => /*#__PURE__*/React.createElement("div", {
    key: title
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-display)",
      fontWeight: 600,
      fontSize: 11,
      letterSpacing: "0.12em",
      textTransform: "uppercase",
      color: "rgba(255,255,255,0.5)",
      marginBottom: 14
    }
  }, title), /*#__PURE__*/React.createElement("ul", {
    style: {
      listStyle: "none",
      padding: 0,
      margin: 0,
      display: "flex",
      flexDirection: "column",
      gap: 10
    }
  }, links.map(l => /*#__PURE__*/React.createElement("li", {
    key: l
  }, /*#__PURE__*/React.createElement("a", {
    href: "#",
    style: {
      color: "rgba(255,255,255,0.7)",
      textDecoration: "none",
      fontSize: "var(--text-sm)"
    }
  }, l))))))), /*#__PURE__*/React.createElement("div", {
    style: {
      borderTop: "1px solid rgba(255,255,255,0.1)",
      paddingTop: "var(--space-5)",
      display: "flex",
      justifyContent: "space-between",
      flexWrap: "wrap",
      gap: 12,
      fontSize: "var(--text-xs)",
      color: "rgba(255,255,255,0.5)"
    }
  }, /*#__PURE__*/React.createElement("span", null, "\xA9 2026 TechXYZ \u2014 Creative Engineering. Tous droits r\xE9serv\xE9s."), /*#__PURE__*/React.createElement("span", {
    style: {
      display: "flex",
      gap: 18
    }
  }, /*#__PURE__*/React.createElement("a", {
    href: "#",
    style: {
      color: "inherit",
      textDecoration: "none"
    }
  }, "Mentions l\xE9gales"), /*#__PURE__*/React.createElement("a", {
    href: "#",
    style: {
      color: "inherit",
      textDecoration: "none"
    }
  }, "Confidentialit\xE9"), /*#__PURE__*/React.createElement("a", {
    href: "#",
    style: {
      color: "inherit",
      textDecoration: "none"
    }
  }, "RGPD")))), /*#__PURE__*/React.createElement("style", null, `@media (max-width: 820px){ .txyz-foot-grid{ grid-template-columns: 1fr 1fr !important; } }`));
}
Object.assign(window, {
  Footer
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/vitrine/Footer.jsx", error: String((e && e.message) || e) }); }

// ui_kits/vitrine/Hero.jsx
try { (() => {
/* TechXYZ vitrine — hero on the ink ground with spark glow */
function Hero({
  onContact
}) {
  const {
    Button,
    Badge
  } = window.TechXYZDesignSystem_ff9a8f;
  return /*#__PURE__*/React.createElement("section", {
    id: "top",
    style: {
      position: "relative",
      overflow: "hidden",
      background: "var(--gradient-mesh)",
      color: "#fff"
    }
  }, /*#__PURE__*/React.createElement("div", {
    "aria-hidden": "true",
    style: {
      position: "absolute",
      inset: 0,
      backgroundImage: "radial-gradient(circle at 1px 1px, rgba(255,255,255,0.05) 1px, transparent 0)",
      backgroundSize: "28px 28px",
      pointerEvents: "none"
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative",
      maxWidth: "var(--container-xl)",
      margin: "0 auto",
      padding: "var(--space-11) var(--space-5)",
      display: "grid",
      gridTemplateColumns: "1.1fr 0.9fr",
      gap: "var(--space-9)",
      alignItems: "center"
    },
    className: "txyz-hero-grid"
  }, /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "inline-flex",
      alignItems: "center",
      gap: 8,
      marginBottom: 20
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-sans)",
      fontWeight: 300,
      fontSize: 13,
      letterSpacing: "0.26em",
      color: "var(--azure-400)"
    }
  }, "CREATIVE ENGINEERING")), /*#__PURE__*/React.createElement("h1", {
    style: {
      fontFamily: "var(--font-sans)",
      fontWeight: 800,
      color: "#fff",
      fontSize: "clamp(2.2rem, 4.6vw, 3.4rem)",
      lineHeight: 1.08,
      letterSpacing: "-0.02em",
      margin: "0 0 20px"
    }
  }, "Des logiciels sur-mesure pour les ", /*#__PURE__*/React.createElement("span", {
    style: {
      color: "var(--azure-400)"
    }
  }, "collectivit\xE9s"), ", les associations et les PME."), /*#__PURE__*/React.createElement("p", {
    style: {
      fontSize: "var(--text-lg)",
      lineHeight: 1.6,
      color: "rgba(255,255,255,0.78)",
      maxWidth: "46ch",
      margin: "0 0 var(--space-7)"
    }
  }, "Nous concevons et d\xE9veloppons des outils simples, fiables et adapt\xE9s \xE0 votre r\xE9alit\xE9 de terrain \u2014 sans jargon, sans superflu."), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 12,
      flexWrap: "wrap"
    }
  }, /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    size: "lg",
    onClick: onContact
  }, "Parlons de votre projet"), /*#__PURE__*/React.createElement(Button, {
    variant: "outline",
    size: "lg",
    style: {
      background: "transparent",
      color: "#fff",
      borderColor: "rgba(255,255,255,0.28)"
    }
  }, "Voir nos r\xE9alisations")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 22,
      marginTop: "var(--space-8)",
      flexWrap: "wrap"
    }
  }, [["building-2", "Mairies & EPCI"], ["users", "Associations"], ["briefcase", "PME"]].map(([ic, t]) => /*#__PURE__*/React.createElement("div", {
    key: t,
    style: {
      display: "flex",
      alignItems: "center",
      gap: 8,
      color: "rgba(255,255,255,0.72)",
      fontSize: "var(--text-sm)",
      fontWeight: 600
    }
  }, /*#__PURE__*/React.createElement("i", {
    "data-lucide": ic,
    style: {
      width: 18,
      height: 18,
      color: "var(--azure-400)"
    }
  }), t)))), /*#__PURE__*/React.createElement("div", {
    className: "txyz-hero-art",
    style: {
      position: "relative"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      background: "#fff",
      borderRadius: "var(--radius-xl)",
      boxShadow: "var(--shadow-xl)",
      overflow: "hidden",
      transform: "rotate(0.5deg)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      height: 44,
      background: "var(--surface-sunken)",
      borderBottom: "1px solid var(--border-subtle)",
      display: "flex",
      alignItems: "center",
      gap: 7,
      padding: "0 16px"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 10,
      height: 10,
      borderRadius: "50%",
      background: "var(--neutral-300)"
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      width: 10,
      height: 10,
      borderRadius: "50%",
      background: "var(--neutral-300)"
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      width: 10,
      height: 10,
      borderRadius: "50%",
      background: "var(--neutral-300)"
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      marginLeft: 10,
      fontFamily: "var(--font-mono)",
      fontSize: 11,
      color: "var(--text-muted)"
    }
  }, "portail.ville-exemple.fr")), /*#__PURE__*/React.createElement("div", {
    style: {
      padding: "var(--space-5)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      marginBottom: 16
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontWeight: 700,
      color: "var(--ink-900)",
      fontSize: 16
    }
  }, "Demandes citoyennes"), /*#__PURE__*/React.createElement(Badge, {
    tone: "success",
    dot: true
  }, "4 nouvelles")), [["État civil", "warning", "En cours"], ["Urbanisme", "success", "Traité"], ["Voirie", "brand", "Reçu"]].map(([t, tone, st], i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      padding: "12px 0",
      borderTop: i ? "1px solid var(--border-subtle)" : "none"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 10
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: 32,
      height: 32,
      borderRadius: 8,
      background: "var(--azure-50)",
      display: "inline-flex",
      alignItems: "center",
      justifyContent: "center",
      color: "var(--azure-600)"
    }
  }, /*#__PURE__*/React.createElement("i", {
    "data-lucide": "file-text",
    style: {
      width: 16,
      height: 16
    }
  })), /*#__PURE__*/React.createElement("span", {
    style: {
      fontWeight: 600,
      color: "var(--text-body)",
      fontSize: 14
    }
  }, t)), /*#__PURE__*/React.createElement(Badge, {
    tone: tone
  }, st))))), /*#__PURE__*/React.createElement("div", {
    "aria-hidden": "true",
    style: {
      position: "absolute",
      inset: "-12% -8% auto auto",
      width: 120,
      height: 120,
      background: "var(--azure-500)",
      filter: "blur(60px)",
      opacity: 0.5,
      borderRadius: "50%",
      zIndex: -1
    }
  }))), /*#__PURE__*/React.createElement("style", null, `@media (max-width: 900px){ .txyz-hero-grid{ grid-template-columns: 1fr !important; } .txyz-hero-art{ display:none; } }`));
}
Object.assign(window, {
  Hero
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/vitrine/Hero.jsx", error: String((e && e.message) || e) }); }

// ui_kits/vitrine/NavBar.jsx
try { (() => {
/* TechXYZ vitrine — top navigation bar */
function NavBar({
  onContact
}) {
  const {
    Button
  } = window.TechXYZDesignSystem_ff9a8f;
  const [open, setOpen] = React.useState(false);
  const links = [["Services", "#services"], ["Pour qui ?", "#audiences"], ["Notre approche", "#process"]];
  return /*#__PURE__*/React.createElement("header", {
    style: {
      position: "sticky",
      top: 0,
      zIndex: 100,
      background: "rgba(255,255,255,0.82)",
      backdropFilter: "var(--blur-glass)",
      WebkitBackdropFilter: "var(--blur-glass)",
      borderBottom: "1px solid var(--border-subtle)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: "var(--container-xl)",
      margin: "0 auto",
      padding: "0 var(--space-5)",
      height: 72,
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between"
    }
  }, /*#__PURE__*/React.createElement("a", {
    href: "#top",
    style: {
      display: "flex",
      alignItems: "center",
      gap: 12,
      textDecoration: "none"
    }
  }, /*#__PURE__*/React.createElement("img", {
    src: "../../assets/logo/techxyz-mark.png",
    alt: "",
    style: {
      width: 40,
      height: 40
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-display)",
      fontWeight: 700,
      fontSize: 22,
      color: "var(--ink-900)",
      letterSpacing: "-0.01em"
    }
  }, "TECH", /*#__PURE__*/React.createElement("span", {
    style: {
      color: "var(--azure-500)",
      fontWeight: 500
    }
  }, "XYZ"))), /*#__PURE__*/React.createElement("nav", {
    className: "txyz-nav-links",
    style: {
      display: "flex",
      alignItems: "center",
      gap: "var(--space-6)"
    }
  }, links.map(([label, href]) => /*#__PURE__*/React.createElement("a", {
    key: href,
    href: href,
    style: {
      fontFamily: "var(--font-sans)",
      fontWeight: 600,
      fontSize: "var(--text-sm)",
      color: "var(--text-body)",
      textDecoration: "none"
    }
  }, label))), /*#__PURE__*/React.createElement("div", {
    className: "txyz-nav-cta",
    style: {
      display: "flex",
      alignItems: "center",
      gap: 12
    }
  }, /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    size: "sm",
    onClick: onContact
  }, "Parlons de votre projet")), /*#__PURE__*/React.createElement("button", {
    className: "txyz-burger",
    "aria-label": "Menu",
    onClick: () => setOpen(!open),
    style: {
      display: "none",
      width: 40,
      height: 40,
      border: "1.5px solid var(--border-default)",
      borderRadius: "var(--radius-md)",
      background: "#fff",
      cursor: "pointer"
    }
  }, /*#__PURE__*/React.createElement("i", {
    "data-lucide": open ? "x" : "menu"
  }))), open && /*#__PURE__*/React.createElement("div", {
    style: {
      padding: "var(--space-3) var(--space-5) var(--space-5)",
      borderTop: "1px solid var(--border-subtle)",
      display: "flex",
      flexDirection: "column",
      gap: 6
    }
  }, links.map(([label, href]) => /*#__PURE__*/React.createElement("a", {
    key: href,
    href: href,
    onClick: () => setOpen(false),
    style: {
      padding: "10px 0",
      fontWeight: 600,
      color: "var(--text-body)",
      textDecoration: "none"
    }
  }, label)), /*#__PURE__*/React.createElement(Button, {
    variant: "primary",
    onClick: () => {
      setOpen(false);
      onContact && onContact();
    },
    style: {
      marginTop: 8
    }
  }, "Parlons de votre projet")), /*#__PURE__*/React.createElement("style", null, `
        @media (max-width: 860px){
          .txyz-nav-links, .txyz-nav-cta{ display:none !important; }
          .txyz-burger{ display:inline-flex !important; align-items:center; justify-content:center; }
        }
      `));
}
Object.assign(window, {
  NavBar
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/vitrine/NavBar.jsx", error: String((e && e.message) || e) }); }

// ui_kits/vitrine/Process.jsx
try { (() => {
/* TechXYZ vitrine — "notre approche" process steps */
function Process() {
  const steps = [["01", "Écouter", "On commence par comprendre votre métier, vos contraintes et vos usagers — sur le terrain."], ["02", "Concevoir", "Maquettes et prototypes validés ensemble avant la première ligne de code. Pas de surprise."], ["03", "Construire", "Développement par étapes courtes, avec des livraisons régulières que vous pouvez tester."], ["04", "Faire vivre", "Formation, support et évolutions. Votre outil s'améliore au fil du temps."]];
  return /*#__PURE__*/React.createElement("section", {
    id: "process",
    style: {
      background: "var(--surface-page)",
      padding: "var(--section-y) var(--space-5)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: "var(--container-xl)",
      margin: "0 auto"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      marginBottom: "var(--space-8)",
      maxWidth: "60ch"
    }
  }, /*#__PURE__*/React.createElement("span", {
    className: "eyebrow"
  }, "Notre approche"), /*#__PURE__*/React.createElement("h2", {
    style: {
      fontSize: "var(--text-3xl)",
      margin: "10px 0 12px",
      color: "var(--ink-900)"
    }
  }, "Le \xAB\xA0Creative Engineering\xA0\xBB, concr\xE8tement"), /*#__PURE__*/React.createElement("p", {
    style: {
      color: "var(--text-muted)",
      fontSize: "var(--text-lg)"
    }
  }, "La rigueur de l'ing\xE9nierie, la souplesse d'un studio \xE0 taille humaine. Quatre \xE9tapes, beaucoup d'\xE9coute.")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "grid",
      gridTemplateColumns: "repeat(auto-fit, minmax(240px, 1fr))",
      gap: "var(--space-5)"
    }
  }, steps.map(([num, title, body]) => /*#__PURE__*/React.createElement("div", {
    key: num,
    style: {
      position: "relative",
      paddingTop: "var(--space-5)"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      position: "absolute",
      top: 0,
      left: 0,
      fontFamily: "var(--font-display)",
      fontWeight: 800,
      fontSize: 40,
      lineHeight: 1,
      color: "var(--azure-200)"
    }
  }, num), /*#__PURE__*/React.createElement("div", {
    style: {
      paddingTop: 28
    }
  }, /*#__PURE__*/React.createElement("h3", {
    style: {
      fontSize: "var(--text-xl)",
      margin: "0 0 8px",
      color: "var(--ink-900)"
    }
  }, title), /*#__PURE__*/React.createElement("p", {
    style: {
      color: "var(--text-muted)",
      fontSize: "var(--text-md)",
      lineHeight: 1.6,
      margin: 0
    }
  }, body)))))));
}
Object.assign(window, {
  Process
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/vitrine/Process.jsx", error: String((e && e.message) || e) }); }

// ui_kits/vitrine/Services.jsx
try { (() => {
/* TechXYZ vitrine — services grid */
function Services() {
  const services = [["pencil-ruler", "Conception sur-mesure", "On part de votre besoin réel, pas d'un modèle générique. Ateliers, maquettes, validation."], ["code-2", "Développement web & mobile", "Des applications robustes, maintenables et accessibles (RGAA), livrées par étapes."], ["plug", "Intégration & reprise de données", "Connexion à vos outils existants et migration de vos données, sans perte ni rupture."], ["life-buoy", "Accompagnement & support", "Formation de vos équipes et support réactif. Nous restons joignables après la livraison."], ["shield-check", "Hébergement souverain & RGPD", "Données hébergées en France, conformité RGPD, sauvegardes et sécurité par défaut."], ["refresh-cw", "Maintenance évolutive", "Votre logiciel grandit avec vous : corrections, nouvelles fonctions, mises à jour."]];
  return /*#__PURE__*/React.createElement("section", {
    id: "services",
    style: {
      background: "#fff",
      padding: "var(--section-y) var(--space-5)",
      borderTop: "1px solid var(--border-subtle)",
      borderBottom: "1px solid var(--border-subtle)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      maxWidth: "var(--container-xl)",
      margin: "0 auto"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      marginBottom: "var(--space-8)"
    }
  }, /*#__PURE__*/React.createElement("span", {
    className: "eyebrow"
  }, "Nos services"), /*#__PURE__*/React.createElement("h2", {
    style: {
      fontSize: "var(--text-3xl)",
      margin: "10px 0 0",
      color: "var(--ink-900)",
      maxWidth: "20ch"
    }
  }, "De l'id\xE9e \xE0 l'outil que vos \xE9quipes utilisent")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "grid",
      gridTemplateColumns: "repeat(auto-fit, minmax(300px, 1fr))",
      gap: "1px",
      background: "var(--border-subtle)",
      border: "1px solid var(--border-subtle)",
      borderRadius: "var(--radius-lg)",
      overflow: "hidden"
    }
  }, services.map(([ic, title, body], i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    className: "txyz-svc",
    style: {
      background: "#fff",
      padding: "var(--space-6)",
      transition: "background var(--duration-fast) var(--ease-standard)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 12,
      marginBottom: 12
    }
  }, /*#__PURE__*/React.createElement("i", {
    "data-lucide": ic,
    style: {
      width: 24,
      height: 24,
      color: "var(--azure-600)",
      strokeWidth: 1.75
    }
  }), /*#__PURE__*/React.createElement("h3", {
    style: {
      fontSize: "var(--text-lg)",
      margin: 0,
      color: "var(--ink-900)"
    }
  }, title)), /*#__PURE__*/React.createElement("p", {
    style: {
      color: "var(--text-muted)",
      fontSize: "var(--text-md)",
      lineHeight: 1.6,
      margin: 0
    }
  }, body))))), /*#__PURE__*/React.createElement("style", null, `.txyz-svc:hover{ background: var(--surface-page) !important; }`));
}
Object.assign(window, {
  Services
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/vitrine/Services.jsx", error: String((e && e.message) || e) }); }

__ds_ns.Button = __ds_scope.Button;

__ds_ns.IconButton = __ds_scope.IconButton;

__ds_ns.Avatar = __ds_scope.Avatar;

__ds_ns.Badge = __ds_scope.Badge;

__ds_ns.Card = __ds_scope.Card;

__ds_ns.Checkbox = __ds_scope.Checkbox;

__ds_ns.Input = __ds_scope.Input;

__ds_ns.Select = __ds_scope.Select;

__ds_ns.Switch = __ds_scope.Switch;

})();
