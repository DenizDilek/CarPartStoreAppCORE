interface PlaceholderImageProps {
  className?: string;
}

export default function PlaceholderImage({ className = '' }: PlaceholderImageProps) {
  return (
    <svg
      className={className}
      viewBox="0 0 400 300"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      {/* Background */}
      <rect width="400" height="300" fill="#F8F9FA" />

      {/* Car Icon */}
      <g transform="translate(140, 100)">
        {/* Car Body */}
        <path
          d="M 10 40 L 20 20 L 100 20 L 110 40 L 10 40 Z"
          fill="#DEE2E6"
          stroke="#ADB5BD"
          strokeWidth="2"
        />
        <rect
          x="5"
          y="40"
          width="110"
          height="30"
          fill="#DEE2E6"
          stroke="#ADB5BD"
          strokeWidth="2"
          rx="3"
        />

        {/* Windows */}
        <rect x="25" y="25" width="25" height="15" fill="#E9ECEF" stroke="#ADB5BD" strokeWidth="1" />
        <rect x="55" y="25" width="30" height="15" fill="#E9ECEF" stroke="#ADB5BD" strokeWidth="1" />

        {/* Wheels */}
        <circle cx="30" cy="75" r="12" fill="#495057" />
        <circle cx="30" cy="75" r="6" fill="#DEE2E6" />
        <circle cx="90" cy="75" r="12" fill="#495057" />
        <circle cx="90" cy="75" r="6" fill="#DEE2E6" />

        {/* Headlights */}
        <rect x="2" y="45" width="8" height="8" fill="#FFD93D" />
        <rect x="110" y="45" width="8" height="8" fill="#FF6B6B" />
      </g>

      {/* Text */}
      <text
        x="200"
        y="220"
        textAnchor="middle"
        fontSize="16"
        fill="#6C757D"
        fontFamily="system-ui"
      >
        No Image Available
      </text>
    </svg>
  );
}
